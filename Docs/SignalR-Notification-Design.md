# NexusStack SignalR 通知推送（MQ 中继）设计文档

> 文档状态：**V1 设计已确认（待实现）**  
> 最后更新：2026-03  
> 涉及分支：`dev`

---

## 一、系统背景与目标

### 背景

NexusStack 后端当前已具备 RabbitMQ 事件总线与后台消费服务：

- 事件发布：业务侧（WebAPI / PlanTaskService）把异步通知写入异步任务（`AsyncTask`），并发布 `NotificationEventData` 到 MQ。
- 事件消费：MQService 进程在启动时通过 `AddRabbitMQEventBus()` 自动订阅所有 `IEventHandler<>`，由 `EventSubscriber` 消费并分发到对应 Handler。
- 通知处理：`NotificationEventHandler` 已预留“使用 SignalR 服务执行发送具体的通知逻辑”的 Todo。

### 目标

建立一套**基于 MQ 发布订阅驱动**的异步消息推送服务，使得：

- WebAPI / PlanTaskService **只负责生产消息**（写入异步任务 + 发布 MQ 事件）
- MQService **专注消费事件并推送**（SignalR 作为实时通道）
- 推送链路具备：鉴权、按用户/分组定向、可横向扩展、可观测、可回滚

### 非目标（V1 不做）

- 不引入复杂工作流（如全量消息中心、统一模板渲染、富文本等）
- 不承诺“恰好一次”投递语义（MQ 消费链路默认至少一次，见第六节）
- 不把所有对外入口强制收敛到 WebAPI（如需收敛见第八节方案 B）

---

## 二、总体架构（推荐方案 A：客户端直连 MQService 的 Hub）

### 2.1 数据与控制流

```mermaid
flowchart LR
  subgraph Producer[生产者（只生产，不推送）]
    WebAPI[WebAPI]
    Plan[PlanTaskService]
  end

  subgraph MQ[消息队列]
    Rabbit[(RabbitMQ Exchange/Queue)]
  end

  subgraph Relay[推送中继]
    MQSvc[MQService\n(EventSubscriber + NotificationEventHandler + SignalR Hub)]
  end

  subgraph Client[客户端]
    App[Web/APP/小程序\nSignalR Client]
  end

  WebAPI -->|Publish NotificationEventData| Rabbit
  Plan -->|Publish NotificationEventData| Rabbit
  Rabbit -->|Consume| MQSvc
  MQSvc -->|SignalR Push| App
```

### 2.2 关键边界

- **业务侧（WebAPI/PlanTaskService）**：不依赖 SignalR，不需要 `IHubContext`，只把“要通知什么、通知谁”写入 `NotificationEventData.Data`。
- **中继侧（MQService）**：消费 MQ 后决定推送路由（User / Group / Broadcast），并通过 `IHubContext<...>` 推送。
- **客户端**：与 MQService 的 Hub 建立连接并携带令牌，服务端按用户身份授权接收通知。

---

## 三、组件设计

### 3.1 SignalR Hub（NotificationHub）

职责：

- 提供客户端连接入口（WebSocket / SSE / LongPolling，优先 WebSocket）
- 识别连接的用户身份（UserIdentifier）
- 维护用户连接与分组（Group）关系（可选）

建议：

- Hub 只做连接与分组管理，不直接处理业务消息生成。
- 服务器端推送统一使用 `IHubContext<NotificationHub>`（不在 Hub 内部发消息）。

### 3.2 NotificationEventHandler（消费后推送）

职责：

- 从 `NotificationEventData` 读取目标与负载
- 进行推送（User / Group / All）
- 将异步任务状态更新为 Completed/Fail（保持现有 `AsyncTaskService` 流程）

注意：

- 推送失败不应阻塞 MQ 消费线程过久；应有超时策略与失败记录（V1 以日志 + AsyncTask.ErrorMessage 为主）。

### 3.3 EventSubscriber（现状能力复用）

当前 `EventSubscriber` 已具备：

- 重试/死信队列（DLQ、Retry Exchange、TTL 等）
- 消费幂等（基于 Redis 的 idempotency key，可配置）

这意味着通知推送链路天然是“**至少一次**”语义：同一条消息可能被重复投递到 Handler。

---

## 四、消息模型（V1 约定）

### 4.1 NotificationEventData 载荷约定

`NotificationEventData.Data` 建议使用 JSON 字符串，统一为：

```json
{
  "target": {
    "type": "user | group | all",
    "userId": "123",
    "group": "tenant:1"
  },
  "event": "Notification",
  "payload": {
    "title": "审批提醒",
    "content": "你有一条待审批",
    "level": "info | warn | error",
    "bizType": "OrderApprove",
    "bizId": "A001",
    "extra": { }
  },
  "meta": {
    "traceId": "可选",
    "dedupeKey": "可选"
  }
}
```

说明：

- `target.type`
  - `user`：按用户定向推送（推荐默认）
  - `group`：按分组推送（用于租户/项目/组织维度）
  - `all`：全员广播（谨慎使用）
- `event`：SignalR 客户端监听的事件名（例如 `Notification`）。
- `payload`：业务负载（保持轻量，避免超大消息）。
- `meta.dedupeKey`：可选，客户端可用于去重展示（见第六节）。

### 4.2 兼容性策略

- V1 优先保证：即使 `Data` 解析失败，也要能记录错误并把任务标记为 Fail，避免消息无限重试。
- 后续若演进字段，按“新增字段向后兼容”原则推进。

---

## 五、鉴权与安全（直连 MQService 必做）

### 5.1 必须做鉴权的原因

若 Hub 允许匿名连接，将产生：

- 未授权订阅与数据泄露（任何人连上即可接收推送）
- 冒充用户（伪造 userId / group）
- 连接滥用 DoS（占满连接与带宽，影响 MQ 消费稳定性）

因此：**客户端直连 MQService 也必须进行认证 + 授权**。

### 5.2 认证策略（建议复用 WebAPI 的 Token 体系）

原则：

- MQService 与 WebAPI **使用同一套令牌签发与校验规则**，确保用户身份一致。
- Hub 的 `UserIdentifier` 必须来自服务端验证后的 `ClaimsPrincipal`（严禁信任客户端传入的 userId）。

落地方向（两种任选其一，取决于你当前 Token 体系）：

- **方案 1：JWT Bearer（通用）**
  - 客户端用 SignalR 的 `accessTokenFactory` 携带 token
  - MQService 配置 `JwtBearer`，并在握手阶段支持从 querystring 读取 `access_token`（WebSocket 常用）
- **方案 2：复用现有自定义认证 Handler**
  - 若 WebAPI 使用自定义 Token 校验（例如 `Authorization-Token` 方案），MQService 需注册同样的 Authentication Scheme
  - 需要确认该方案是否支持 WebSocket 握手阶段传递令牌（Header 或 querystring）
  - 推荐实践：**为 SignalR 单独增加一个认证 Scheme/Handler**，避免改动既有 HTTP 认证行为
    - 新增 Scheme：例如 `Authorization-SignalR-Token`
    - 新增 Handler：例如 `RequestAuthenticationSignalRTokenHandler`
      - 优先从 `Request.Query["access_token"]` 取 token（浏览器 WebSocket/SignalR 常见）
      - 回退从 `Request.Headers.Authorization` 取 `Bearer ...`（兼容非浏览器客户端）
      - 复用现有 `ValidateTokenAsync` 与用户上下文缓存构建逻辑，确保 Claims（如 `ClaimTypes.NameIdentifier`）一致
    - 在 Hub 上指定认证方案：`[Authorize(AuthenticationSchemes = "Authorization-SignalR-Token")]`
    - 安全注意：仅在 **HTTPS/WSS** 下启用 querystring token，且代理/日志需避免记录或泄露完整查询串（可做脱敏）

### 5.3 授权与隔离建议

- 只允许连接加入“自己有权”的 group（例如 tenant/project），group 名称由服务端依据 Claim 计算。
- 对广播（all）设置更高权限要求（仅系统管理员可触发生产消息）。
- 生产侧（WebAPI/PlanTaskService）发布通知前可做权限校验；中继侧（MQService）也可做二次校验（V1 可先只做身份校验，授权作为增强项）。

### 5.4 传输与防护

最低要求：

- **HTTPS/WSS**（TLS）
- 配置允许的 Origin / CORS（尤其浏览器端）
- 连接数、消息频率限制（至少可观测与告警，必要时加限流）

---

## 六、可靠性与投递语义

### 6.1 至少一次（At-least-once）

由于 `EventSubscriber` 存在重试与幂等机制，整体语义为：

- MQ → Handler：**至少一次**
- Handler → 客户端：网络不可靠，可能丢失、可能重复

因此客户端应具备**幂等展示**能力（可选但强烈建议）：

- 使用 `EventBase.Id`（或 `meta.dedupeKey`）作为消息唯一键
- UI 层按唯一键去重（例如 5~10 分钟窗口）

### 6.2 失败策略（V1）

推送失败时：

- 记录错误到 `AsyncTask.ErrorMessage`，并将任务置为 Fail（保持现有实现风格）
- 由 MQ 重试机制决定是否重投（取决于 Handler 抛异常与否）

建议（V1 取最小改动）：

- 解析失败（Data 非法）视为不可重试：直接 Fail，避免无限重试污染队列
- 网络瞬时失败可重试：让异常抛出以触发 MQ 重试

---

## 七、部署与扩展

### 7.1 单实例 MQService（V1 默认）

优点：实现简单，无需 Backplane。

约束：MQService 宕机将导致实时推送不可用（但 MQ 消息仍会堆积等待恢复后消费）。

### 7.2 多实例 MQService（需要 Backplane）

当 MQService 横向扩展为多个实例时，需要解决：

- 客户端连接随机落到不同实例
- 事件消费由任一实例触发推送，必须能把消息投递到“持有连接的实例”

解决：引入 SignalR Backplane（推荐 Redis）。

> 注：NexusStack 已引入 Redis 作为基础设施，优先复用现有 Redis 能力。

---

## 八、替代方案（方案 B：客户端只连 WebAPI，MQService 仅负责发到 Backplane）

适用场景：

- 希望“所有对外入口统一在 WebAPI / Gateway”
- 希望认证、限流、审计全部集中在 WebAPI 层

架构要点：

- WebAPI：托管 Hub，客户端只连 WebAPI
- MQService：不对外暴露 Hub（可选），但仍需要 `IHubContext` 并连接同一个 Backplane
- WebAPI 与 MQService：共享 Redis Backplane，从而实现跨实例推送

取舍：

- 优点：对外入口单一，安全与运维集中
- 代价：部署复杂度更高（Backplane 依赖更强）

---

## 九、落地清单（V1 实施步骤）

### 9.1 代码改动点（最小必要）

- MQService：
  - 启用 SignalR（仅在 `CoreServiceType.MQService` 分支）
  - 增加 `NotificationHub` 并映射路由（例如 `/hubs/notification`）
  - 注册认证（JWT 或复用现有 Token 方案），确保 Hub 可获取 `Context.User`
  - 若采用自定义 `Authorization-Token`：建议为 Hub 单独注册 `Authorization-SignalR-Token` Scheme/Handler，以兼容 `access_token` 且不影响既有 HTTP 认证
- Core：
  - 在 `NotificationEventHandler` 中完成：解析 `NotificationEventData.Data` → `IHubContext` 推送 → 更新 AsyncTask 状态
- Producer（WebAPI/PlanTaskService）：
  - 按第 4 节 JSON 约定填充 `Data`（保持轻量）

### 9.2 配置项（建议）

- SignalR 路由：`/hubs/notification`
- 允许的 Origin 列表
- Backplane（可选，V1 不必启用）
- 连接与消息相关的限制参数（如最大连接数/频率，后续增强）

---

## 十、风险与回滚

### 10.1 风险

- **未做鉴权**导致的通知泄露（最高优先级风险）
- 推送引入的资源消耗影响 MQ 消费吞吐（需监控连接数、CPU、内存、GC、网络）
- 多实例下未启用 Backplane 导致“部分用户收不到消息”

### 10.2 回滚策略

- 关闭 MQService 的 SignalR 配置与 Hub 映射（保留 MQ 消费逻辑）
- 将 `NotificationEventHandler` 推送逻辑降级为仅记录日志与任务状态（不推送）

---

## 十一、开放问题（实现前需要最终确认）

1. **统一身份标识字段**：用哪个 Claim 作为 `UserIdentifier`（UserId / sub / nameidentifier）？
2. **Token 体系**：当前 WebAPI 的认证方式是否是 JWT，还是自定义 `Authorization-Token`？SignalR 连接 token 准备走 Header 还是 querystring 的 `access_token`（若走 `access_token`，建议使用专用 Scheme/Handler）？
3. **推送路由策略**：V1 只按 userId 推送，还是需要 group（租户/组织）？

