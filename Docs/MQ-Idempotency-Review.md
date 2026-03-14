# MQ 消费幂等实现检查报告

## 1. 现有实现概览

- **幂等键**：`mq:idempotent:{queueDimension}:{messageId}`
  - `queueDimension`：消费者队列名（EventHandler 全名）
  - `messageId`：优先 `BasicProperties.MessageId` → `CorrelationId` → 消息体 UTF-8 的 SHA256
- **获取方式**：Redis `SET key value EXPIRE NX`，成功即“获得幂等”，失败即“重复”
- **行为**：重复则 ACK 并跳过；失败时删除幂等键后重试/进 DLQ；业务成功但 ACK 失败时保留幂等键，依赖重投后去重

## 2. 结论：整体正确，已做加固（五轮检查）

| 检查项 | 结论 |
|--------|------|
| 重复消息（重投/并发） | 正确：仅一个 Nx 成功，其余 DUPLICATE → ACK 跳过 |
| 重试链幂等 | 正确：失败时 Delete 幂等键，重试消息保留 MessageId，再次消费可重新获得幂等 |
| 业务成功但 ACK 失败 | 正确：不 Nack、保留幂等键，重投后 DUPLICATE → ACK 跳过 |
| 未启用幂等 | 正确：返回空串，不写 Redis，HandleFailure 不删 key |
| 多 Handler 同事件 | 正确：按队列维度隔离；queueDimension 在入口固定并贯穿重试，避免竞态 |
| 幂等键 TTL | 正确：ConsumerIdempotencyExpireHours ≤ 0 时按 1 小时处理 |
| 重试发布路由 | 正确：RepublishForRetry 使用入口传入的 queueName，与幂等维度一致，避免误路由 |
| Redis 删除失败 | 正确：DeleteAsync 抛错时不再重试、直接进 DLQ，避免重试消息被误判 DUPLICATE 导致丢失 |
| 重试后 ACK 失败 | 正确：重试已发布但 ACK 抛错时 catch 后走 Nack 进 DLQ，避免原消息处于未 ACK 被重投、与 retry 副本并存导致重复处理 |

**第一轮**：发布端 `BuildMessageId` 在 Id 为空时用 Guid，保证 MessageId 唯一。

**第二轮**：① 入口处固定 `queueDimension` 并传入 `TryAcquireIdempotencyAsync`；② 幂等键 TTL ≤ 0 时按 1 小时；③ SHA256 降级注释。

**第三轮**：① `HandleFailureAsync` / `RepublishForRetryAsync` 改为接收并使用入口的 `queueDimension` 作为重试路由的 queueName；② `HandleFailureAsync` 内对 `DeleteAsync` 做 try/catch，删除失败时不再重试、直接 Nack 进 DLQ。

**第四轮**：重试发布成功后对 `BasicAckAsync` 做 try/catch，ACK 失败时记录日志并 fall through 到最终 Nack，将当前消息送 DLQ，避免原消息处于“未 ACK”被 broker 重投、与已进入 retry 队列的副本并存导致重复处理。

**第五轮**：复验各分支与边界（未启用幂等、DUPLICATE/ACQUIRED、TryAcquire 抛错、ProcessEvent 抛错、BasicProperties 为空、MaxRetryCount≤0 等），未发现新问题；与前述结论一致。

## 3. 已做改动汇总

- **EventPublisher**：`BuildMessageId`，Id 为空时用 Guid；普通/延迟发布均使用。
- **EventSubscriber**：① 入口解析 `queueDimension` 并传入幂等与失败处理；② `TryAcquireIdempotencyAsync(eventArgs, message, queueDimension)`；③ 幂等键 TTL ≤ 0 按 1 小时；④ `HandleFailureAsync(..., queueNameForRetry, ...)`，幂等键删除失败时 canRetry=false、走 DLQ；⑤ `RepublishForRetryAsync(eventArgs, nextRetryCount, queueName)` 使用传入 queueName；⑥ 重试发布成功后 ACK 包一层 try/catch，ACK 失败时走 Nack 进 DLQ；⑦ SHA256 降级注释。

## 4. 风险与回滚

- 发布端：仅影响 MessageId 生成；回滚即还原为原先的 messageId 表达式。
- 订阅端：行为兼容；回滚需还原 queueDimension 的提前解析与参数传递，以及 TTL 的 0 保护。

---

## 5. 最终审查（Final Audit）

### 5.1 审查范围

- **EventSubscriber.cs**：`OnConsumerMessageReceived` → 幂等获取 → `ProcessEvent` → `HandleFailureAsync` / `RepublishForRetryAsync` 全链路。
- **EventPublisher.cs**：`BuildMessageId` 及普通/延迟发布的 MessageId、CorrelationId 设置。
- **RabbitOptions**：`EnableConsumerIdempotency`、`ConsumerIdempotencyExpireHours`、`MaxRetryCount` 的使用与边界。
- **与文档一致性**：本报告 §1–§4 与当前代码一致。

### 5.2 设计要点确认

| 要点 | 状态 |
|------|------|
| 幂等键唯一性 | 由 queueDimension（队列名）+ messageId（发布端/降级 SHA256）保证；发布端 Id 为空时用 Guid。 |
| 并发安全 | Redis SET Nx 原子；queueDimension 在入口一次解析并贯穿，无后续映射竞态。 |
| 至少一次语义 | 未 ACK 前不删幂等键；业务成功但 ACK 失败保留键，依赖重投 DUPLICATE 去重。 |
| 失败与重试 | 删键 → 重试发布（保留 MessageId）→ ACK；删键失败或重试发布/ACK 失败 → Nack 进 DLQ。 |
| 重试路由正确性 | 重试使用与幂等相同的 queueName（入口传入），确保进入对应 retry 队列。 |

### 5.3 路径矩阵（简要）

| 入口条件 | 幂等结果 | 业务结果 | 出口 |
|----------|----------|----------|------|
| 无 channel/handler 映射 | - | - | Nack → DLQ |
| 幂等关 | - | 成功/失败 | 成功→ACK；失败→删键(无)/重试或 Nack |
| 幂等开 DUPLICATE | - | - | ACK 并跳过 |
| 幂等开 ACQUIRED | 成功 | - | ACK |
| 幂等开 ACQUIRED | 失败 | - | 删键→重试(成功则 ACK，ACK 失败则 Nack) 或 Nack |
| 幂等开 ACQUIRED | 异常(含 ACK 失败) | 已成功 | 不 Nack，保留键，依赖重投去重 |
| 幂等开 ACQUIRED | 异常 | 未成功 | 删键→重试或 Nack |

### 5.4 残余风险与假设

- **Redis 不可用**：TryAcquire 抛错 → 不占键，会重试或 Nack，可能重复处理（符合至少一次）；Delete 抛错 → 不重试、进 DLQ，不丢消息。
- **queueDimension 退化为 RoutingKey**：仅在入口即无映射时发生，重试发布会使用同一错误维度，retry 可能无法路由；属异常配置/生命周期边界，已通过入口统一解析尽量规避。
- **Broker 重投语义**：假定未 ACK 的消息可能被再次投递；业务成功但 ACK 失败依赖该语义实现去重。
- **MessageId/CorrelationId 类型**：假定 RabbitMQ.Client 中为 string；若为 byte[] 需调用方另行处理。

### 5.5 审查结论

- **代码与文档**：实现与 §1–§4 描述一致，五轮检查结论成立。
- **最终结论**：当前 MQ 消费幂等与失败重试逻辑**通过最终审查**，无新增修改建议；可据此作为上线前幂等与重试行为的依据。

### 5.6 与业务“手动重试”的配合

- **场景**：AsyncTask 失败后通过 `AsyncTaskService.RetryAsync` 重新发布 MQ 消息；事件体仍是同一 TaskCode + TaskId，若 MessageId 仍为 `TaskCode:TaskId`，会命中首次消费时写入的幂等键，被判 DUPLICATE 并跳过，导致重试消息不执行。
- **处理**：发布接口支持可选 `messageIdOverride`；`RetryAsync` 调用时传入 `{task.Code}:{task.Id}:retry:{Guid}` 作为 MessageId，使重试消息使用独立幂等键，不被首次消费的幂等键拦截。

---

## 6. 延迟队列（TTL+DLX）审查

### 6.1 实现概览

- **方式**：RabbitMQ 原生 TTL + 死信交换机（DLX），固定档位（1/3/5/10/30/60/120 分钟）。
- **流程**：消息发到延迟交换机（如 `ExchangeName.delayed`）→ 进入延迟队列 `delay.{tierKey}.{事件类型}`（队列级 `x-message-ttl`）→ 无消费者，到期后死信到主交换机，routing key = 事件全名 → 由现有业务队列消费。
- **代码位置**：`EventPublisher.PublishDelayedInternalAsync`、`EnsureDelayedExchangeAndQueueAsync`。

### 6.2 结论：无堵塞风险，实现正确

| 检查项 | 结论 |
|--------|------|
| **队头阻塞（同一队列混用多档 TTL）** | **无**：每个延迟队列只对应一个档位（如 `delay.1m.XXX`），队列级 TTL 固定，同队列表头到期顺序与入队顺序一致，不存在“长 TTL 挡短 TTL”的队头阻塞。 |
| **延迟队列是否有消费者** | **无消费者**：延迟队列仅用于暂存，消息只通过 TTL 过期后经 DLX 转投主交换机，不会被消费逻辑拖慢或堵塞。 |
| **主业务队列是否被堵** | **不会**：主业务队列与延迟队列分离；延迟消息到期后按事件类型路由到主交换机和现有绑定，与普通消息一样被消费，无额外阻塞。 |
| **routing key 与绑定** | **一致**：发布用 `tierKey.sanitizedEventType`，队列绑定同一 key；DLX 使用 `eventTypeName`（全名），与订阅端 binding 一致，到期后能正确投递。 |
| **并发声明** | **安全**：多线程同时为同一 `(tierKey, eventType)` 声明时可能重复调用 `QueueDeclare`/`QueueBind`，RabbitMQ 声明幂等、参数一致即成功，无错误；仅可能多一次网络调用。 |

### 6.3 可选说明

- **队列数量**：延迟档位 × 事件类型数 = 延迟队列个数；档位与事件类型较多时队列数会增多，属预期，可按需收敛档位或事件类型。
- **到期顺序**：同一延迟队列内多条消息按队头顺序依次过期、再经 DLX 投递，不会“同时全部过期”导致主交换机瞬时压力异常，除非业务侧短时间集中发大量同档延迟消息。
