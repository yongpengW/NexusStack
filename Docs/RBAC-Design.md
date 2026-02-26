# NexusStack RBAC 权限系统设计文档

> 文档状态：**V1 设计已定稿（可迭代优化）**  
> 最后更新：2026-02  
> 涉及分支：`dev`

---

## 一、系统背景与目标

NexusStack 是一个全新架构的后端框架模板，**不存在历史数据迁移约束**，可以按最佳实践设计。

### 业务场景
- 多平台支持：Web 管理端、Android 移动端、微信小程序等
- 组织结构：以 `Region` 表统一承载区域/部门/公司/分支机构等组织单元（目前不单独拆 `Department` 表）
- 用户可绑定到具体组织单元（Region 记录），或绑定上级 Region（自动获得该 Region 下所有子级组织的数据访问权）
- 用户可同时拥有一个或多个角色（Role）
- 角色绑定一个或多个平台
- Permission 表记录每个角色对菜单/操作的权限，以及数据范围（DataRange）

---

### 设计原则（Best Practices）

- **最小授权 + 并集语义**：  
  - 授权以"最小可复用角色"为单位配置；  
  - 用户最终权限始终为多个角色的**并集**，不引入"切换角色"等叠加心智负担。
- **平台上下文强约束**：  
  - 所有权限校验都运行在明确的 `PlatformType` 上下文中；  
  - 同一用户在不同平台的权限完全隔离，便于按端治理。
- **权限 = 显式存在的记录**：  
  - 不设计"否定型权限（Deny）"表；  
  - **有记录即授权**，删除记录即收回权限，避免多处布尔标记产生歧义。
- **读写解耦、缓存优先**：  
  - 认证只关心身份（User + Platform）；  
  - 授权从 Redis 等缓存中按 `(UserId, PlatformType)` 读取预计算结果，DB 只做重建。
- **可演进的数据范围模型**：  
  - `DataRange` 只刻画"数据集合大小"这一维度，按"越小越严格"排序；  
  - 多角色合并时统一采用**最宽松策略**（数值最小），并通过运营规范控制异常组合。

### 成功标准（对业务/技术同样可解释）

- **对业务同学**：能用几句话说明"为什么某个用户在某平台下能/不能看到某条数据"。  
- **对开发同学**：  
  - 授权逻辑集中在少量组件（Filter + Cache Service），无散落硬编码；  
  - 表设计能自然支撑"多平台 + 多角色 + 层级组织 + 数据范围"四个维度；  
  - 后续扩展（新平台、新 DataRange、新菜单）只需增配数据，无需重新设计模型。

## 二、当前代码中识别的反模式

### 反模式 1：字符串存储多值关系

| 位置 | 当前实现 | 问题 |
|---|---|---|
| `User.DepartmentIds` | `string? = "101.203.305"` | 无法索引、无外键约束、解析开销大 |
| `Role.Platforms` | `string = "0,1,2"` | 同上，且无法给平台添加额外元数据 |

**已调整**：在第四节中统一改为 `UserDepartment` 关联表与 `[Flags]` 平台枚举，见决策 A/B。

### 反模式 2：多余的单角色 Token

`UserToken.RoleId` 只记录一个"当前激活角色"，与"用户可拥有多角色"的设计矛盾，
是 **Switch Role** 功能的遗留设计，在方案二中需要调整。

---

## 三、核心设计决策

### 决策 1：权限取并集（Union），去掉"切换角色"功能

**反对切换角色的理由**：
- 增加 UX 复杂度（用户需要手动切换才能使用不同权限）
- 对于互补型角色（如"财务查看" + "HR 编辑"），切换毫无意义
- 权限模型的正确语义是：拥有角色 = 拥有该角色的全部权限

### 决策 2：采用"平台上下文"驱动权限（方案二）

用户登录时**必须指定 PlatformType**，系统自动筛选该用户在当前平台下的有效角色，
权限 = 这些角色的菜单权限**并集**。用户无需感知角色概念。

```
用户登录 WebAdmin
  → 筛选 UserRole 中 Role.Platforms 包含 Web 的角色
  → 取这些角色在 Permission 表中的菜单权限并集
  → 生成 Token，绑定 UserId + PlatformType

请求到达
  → 从 Token 解析 UserId + PlatformType
  → 缓存中查找 (UserId, PlatformType) 对应的权限集合
  → 校验当前 API 是否在权限集合中
```

---

## 四、关键表设计（V1 已确认）

### 4.1 User 表

```csharp
public class User : AuditedEntity
{
    [Required][MaxLength(128)] public string UserName { get; set; } = string.Empty;
    [Required][MaxLength(120)] public string Email    { get; set; } = string.Empty;
    [MaxLength(15)]            public string? Mobile  { get; set; }
    [MaxLength(128)]           public string? RealName { get; set; }
    [MaxLength(128)]           public string? NickName { get; set; }
    [MaxLength(256)]           public string Password     { get; set; } = string.Empty;
    [MaxLength(256)]           public string PasswordSalt { get; set; } = string.Empty;
    public bool     IsEnable       { get; set; } = true;
    public Gender   Gender         { get; set; }
    public DateTime LastLoginTime  { get; set; }
    [MaxLength(512)] public string? Avatar       { get; set; }
    [MaxLength(512)] public string? SignatureUrl { get; set; }

    // 导航属性
    public virtual List<UserDepartment>? UserDepartments { get; set; }  // ← 替换 DepartmentIds 字符串
    public virtual List<UserRole>?       UserRoles       { get; set; }
}
```

> ✅ **决策 A（组织归属建模）**：  
> - `User.DepartmentIds` 字符串字段**必须**改为 `UserDepartment(UserId, DepartmentId)` 关联表；  
> - `DepartmentId` 统一指向组织层级表（当前实现为 `Region.Id`），后续如引入更细分的 Department 表，可通过视图或中间表平滑迁移。

### 4.2 UserDepartment 表（新增，替换 User.DepartmentIds）

```csharp
public class UserDepartment : AuditedEntity
{
    public long UserId       { get; set; }
    public long DepartmentId { get; set; }  // 指向 Region.Id（当前用 Region 统一承载组织单元）

    public virtual User?   User       { get; set; }
    public virtual Region? Department { get; set; }   // 后续如拆出 Department/OrgUnit，可调整为对应实体
}
```

### 4.3 Role 表

```csharp
public class Role : AuditedEntity
{
    [Required][MaxLength(64)] public string Name { get; set; } = string.Empty;
    [Required][MaxLength(64)] public string Code { get; set; } = string.Empty;
    [MaxLength(64)]           public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsEnable { get; set; } = true;
    public int  Order    { get; set; }
    public PlatformType Platforms { get; set; }  // 采用 [Flags] 平台枚举（见决策 B）

    // 导航属性
    public virtual List<UserRole>?     UserRoles     { get; set; }
    public virtual List<Permission>?   Permissions   { get; set; }
}
```

> ✅ **决策 B（平台归属建模）**：  
> - 当前平台数量有限，且暂不需要为"角色-平台"关系单独挂载元数据，选择 **B1：`[Flags]` 枚举**：  
>   ```csharp
>   [Flags]
>   public enum PlatformType { Web = 1, Android = 2, WeChat = 4, iOS = 8 }
>   public PlatformType Platforms { get; set; }
>   ```  
> - 如后续需要为不同平台配置细粒度策略（例如 `MinVersion`、灰度标记、启用状态等），可演进为 **B2：`RolePlatform` 关联表**，迁移路径为：  
>   1. 新增 `RolePlatform` 表并同步生成数据；  
>   2. 代码读取从 `Platforms` → `RolePlatform` 渐进切换；  
>   3. 历史数据完全迁移后再下线 `Platforms` 字段。

### 4.4 UserRole 表

```csharp
public class UserRole : AuditedEntity
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
    // 注意：去掉 IsDefault 字段（方案二不需要默认角色概念）

    public virtual User? User { get; set; }
    public virtual Role? Role { get; set; }
}
```

### 4.5 Permission 表

```csharp
public class Permission : AuditedEntity
{
    public long      RoleId    { get; set; }
    public long      MenuId    { get; set; }
    public DataRange DataRange { get; set; }  // ← 见决策 C

    public virtual Role? Role { get; set; }
    public virtual Menu? Menu { get; set; }
}

public enum DataRange
{
    All                    = 0,  // 所有数据（最宽松）
    CurrentAndSubLevels    = 1,  // 当前及下级
    CurrentLevel           = 2,  // 当前区域级别
    CurrentAndParentLevels = 3,  // 当前及上级（通常不与 Sub 组合出现）
    Self                   = 4,  // 仅自己（最严格）
}
```

> ✅ **决策 C（多角色数据范围合并规则）**：  
> - 统一采用**最宽松策略**：同一用户在某平台下拥有多个角色时，最终 `DataRange` 为**所有角色中数值最小的那个**（因为枚举值越小数据范围越大）；  
>   - 例如：一个角色 `Self`，另一个角色 `CurrentAndSubLevels` → 最终为 `CurrentAndSubLevels`；  
> - 这样与"权限取并集"的核心语义一致：**多给角色只会扩大、不会缩小用户能看到的数据范围**；  
> - 运营侧需配合一条治理约束：  
>   - 同一平台下，如某些角色本身就要求极严格的数据范围（如审计员），应避免与"极宽松"角色（如超级管理员）给同一用户组合使用，必要时通过独立账号解决。

### 4.6 UserToken 表（调整）

```csharp
public class UserToken : AuditedEntity
{
    public long         UserId       { get; set; }
    public PlatformType PlatformType { get; set; }   // ← 核心：绑定平台上下文

    [Required][MaxLength(256)] public string Token        { get; set; } = string.Empty;
    [Required][MaxLength(32)]  public string TokenHash    { get; set; } = string.Empty;
    [Required][MaxLength(256)] public string RefreshToken { get; set; } = string.Empty;

    public DateTime ExpirationDate         { get; set; }
    public bool     RefreshTokenIsAvailable { get; set; }
    public LoginType LoginType             { get; set; }

    // 权限语义上彻底移除 RoleId 字段（方案二不再需要单一激活角色）；
    // 如前端/日志需要展示"主身份"，建议改为：在 User 表或扩展表中维护 PrimaryRoleId，仅用于展示，不参与鉴权。

    [MaxLength(32)]   public string IpAddress { get; set; } = string.Empty;
    [MaxLength(1024)] public string UserAgent { get; set; } = string.Empty;
}
```

---

## 五、方案二权限校验流程

### 5.1 登录阶段

```
POST /api/Token/password  { userName, password, platformType }
  ↓
1. 验证账号密码
2. 筛选 UserRole 中 Role 的 Platforms 包含 platformType 的角色
3. 如果没有任何角色有当前平台权限 → 403 无权限登录此平台
4. 生成 Token，写入 UserToken(UserId, PlatformType)
5. 将 (UserId, PlatformType) 对应的权限集合写入 Redis 缓存
```

### 5.2 请求阶段（RequestAuthorizeFilter）

```
请求到达（携带 Token）
  ↓
1. [AllowAnonymous] → 直接放行
2. Token 认证 → 解析 UserId + PlatformType
3. IsAuthenticated != true → 401
4. IsEnable == false → 403 用户已禁用
5. 读取权限缓存 key = (UserId, PlatformType)
   └─ 缓存未命中 → 从 DB 构建缓存
6. 当前 API (ControllerName + ActionName + HttpMethod)
   是否在权限集合中 → 否 → 403 暂无权限
7. 放行
```

### 5.3 权限缓存结构

```csharp
// Redis Key: "nexusstack:perms:{userId}:{platformType}"
// Value: HashSet<string> apiKeys = { "User:GetAsync:GET", "User:PostAsync:POST", ... }

// 构建逻辑：
var roleIds = UserRole
    .Where(ur => ur.UserId == userId && ur.Role.Platforms 包含 platformType)
    .Select(ur => ur.RoleId);

var menuIds = Permission
    .Where(p => roleIds.Contains(p.RoleId))  // 有记录即授权
    .Select(p => p.MenuId)
    .Distinct();

var apiKeys = MenuResource
    .Where(mr => menuIds.Contains(mr.MenuId))
    .Join(ApiResource, mr => mr.ApiResourceId, ar => ar.Id, (mr, ar) =>
        $"{ar.ControllerName}:{ar.ActionName}:{ar.RequestMethod}")
    .ToHashSet();
```

#### 5.3.1 权限缓存 Key 设计补充说明

- **Key 维度**：`(UserId, PlatformType)`，不直接以 `RoleId` 为 Key，原因：  
  - 平台上下文会影响可用角色集合；  
  - 用户多角色并集后的结果远比单一角色更贴近真实权限视角。  
- **Value 内容**：  
  - API 粒度的白名单（`Controller:Action:Method`）；  
  - 后续如需支持前端按钮/字段级权限，可扩展为：  
    - 再加一组 `HashSet<string> uiPermissions`，由前端约定编码（如 `user.create`, `user.resetPassword`）；  
    - 但仍建议所有后端鉴权以 API 粒度为准，UI 权限仅作为展示/交互约束。

### 5.4 缓存失效时机

| 触发事件 | 失效范围 |
|---|---|
| 管理员修改角色权限 | 所有拥有该角色的用户的缓存 |
| 管理员给用户增减角色 | 该用户的缓存 |
| 用户被禁用 | 该用户所有 Token |
| 菜单/ApiResource 变更 | 全量缓存失效 |

---

## 六、资源建模补充（Menu / ApiResource / MenuResource）

### 6.1 Menu（菜单/功能）表

- **职责**：描述在前端可见的"菜单、功能入口、按钮"等资源。  
- **关键字段建议**：
  - `Id`：主键  
  - `ParentId`：父级菜单 Id（0/NULL 表示根）  
  - `Name`：显示名称  
  - `Code`：稳定业务编码（如 `user.manage`），前端/运营可复用  
  - `Type`：枚举（目录/菜单/按钮/其他）  
  - `Order`：排序  

**当前实现示例（`Menu` 实体）：**

```csharp
public class Menu : AuditedEntity
{
    [MaxLength(256)]
    public string Name { get; set; }

    [MaxLength(256)]
    public string Code { get; set; }

    public long       ParentId     { get; set; }
    public MenuType   Type         { get; set; }
    public PlatformType PlatformType { get; set; }

    [MaxLength(1024)]
    public string Icon { get; set; }
    public MenuIconType IconType { get; set; }

    [MaxLength(1024)]
    public string ActiveIcon { get; set; }
    public MenuIconType ActiveIconType { get; set; }

    [MaxLength(1024)]
    public string Url { get; set; }

    public int  Order        { get; set; }
    public bool IsVisible    { get; set; }
    public bool IsExternalLink { get; set; }

    [MaxLength(1024)]
    public string IdSequences { get; set; }

    public virtual List<Menu>           Children  { get; set; }
    public virtual Menu                 Parent    { get; set; }
    public virtual IEnumerable<MenuResource> Resources { get; set; }

    public long SystemId { get; set; } = 0;
}
```

### 6.2 ApiResource（API 资源）表

- **职责**：为每个需要做权限控制的后端 API 建立**稳定的资源编号**。  
- **关键字段建议**：
  - `Id`：主键  
  - `Code`：稳定编码（如 `user.get`, `user.create`），便于审计日志与菜单关联  
  - `ControllerName` / `ActionName` / `RequestMethod`：用于构建校验用的 `apiKey`  
  - `RouteTemplate`：可选，用于调试与文档生成  

**当前实现示例（`ApiResource` 实体）：**

```csharp
public class ApiResource : AuditedEntity
{
    [MaxLength(256)]
    public string? Name { get; set; }

    [MaxLength(256)]
    public string? Code { get; set; }

    [MaxLength(256)]
    public string? GroupName { get; set; }

    [MaxLength(256)]
    public string? RoutePattern { get; set; }

    [MaxLength(256)]
    public string? NameSpace { get; set; }

    [MaxLength(256)]
    public string? ControllerName { get; set; }

    [MaxLength(256)]
    public string? ActionName { get; set; }

    [MaxLength(256)]
    public string? RequestMethod { get; set; }
}
```

### 6.3 MenuResource（菜单与 API 关联表）

- **职责**：描述"某个菜单/按钮会调用哪些受控 API"。  
- **关键字段建议**：
  - `MenuId`  
  - `ApiResourceId`  
  - 复合唯一索引 `(MenuId, ApiResourceId)`，避免重复配置。  

**当前实现示例（`MenuResource` 实体）：**

```csharp
public class MenuResource : AuditedEntity
{
    public long MenuId       { get; set; }
    public long ApiResourceId { get; set; }

    public virtual Menu       Menu       { get; set; }
    public virtual ApiResource ApiResource { get; set; }
}
```

> **最佳实践**：  
> - 角色只直接配置到 `Menu` 上；`MenuResource` 再把菜单映射到一个或多个 `ApiResource`；  
> - 如此可保持"菜单结构调整"与"权限配置"相对解耦：新增菜单只需挂接已有 `ApiResource` 即可。

---

## 七、ICurrentUser 接口调整

基于方案二，`ICurrentUser` 的关键字段应调整为：

```csharp
public interface ICurrentUser
{
    long   UserId       { get; }   // 用户 ID
    string UserName     { get; }
    string Email        { get; }
    bool   IsAuthenticated { get; }
    bool   IsEnable     { get; }
    string Token        { get; }
    long   TokenId      { get; }
    int    PlatformType { get; }   // 当前登录平台（核心）

    // 以下由权限缓存动态提供，不存储在 Claims 中
    // → 去掉 Roles[], Shops[], Regions[] 等从 Claims 读取的字段
    //    改为通过 IPermissionCacheService 按需查询
}
```

> ⚠️ 注意：当前 `Roles[]`、`Shops[]`、`Regions[]` 是从 Claims 读取的，
> 但认证 Handler 实际上从未正确写入这些 Claims（已知问题）。
> 方案二不再依赖 Claims 传递权限数据，统一走 Redis 缓存。

---

## 八、设计决策汇总

| # | 问题 | 结论 | 说明 |
|---|---|---|---|
| A | `User.DepartmentIds` 如何建模 | ✅ 使用 `UserDepartment(UserId, DepartmentId)` 关联表 | 支持多组织、多层级，具备外键与索引能力 |
| B | `Role.Platforms` 的存储方式 | ✅ 采用 `[Flags] enum`，后续可演进为 `RolePlatform` | 当前平台有限且无复杂元数据需求，保持模型简单 |
| C | 多角色 `DataRange` 冲突 | ✅ 采用"最宽松"策略（取数值最小的 `DataRange`） | 与权限并集语义一致，更多角色 = 更大可见范围 |
| D | `UserToken.RoleId` 是否保留 | ✅ 鉴权语义完全移除，仅在需要时通过其他字段表达"主身份" | Token 只关心 User + Platform，降低耦合 |
| E | `Permission.HasPermission` bool 是否保留 | ✅ 删除 `HasPermission`，有记录即有权限 | 简化模型，避免"存在记录但 HasPermission=false"等含糊状态 |
| F | 是否单独创建 `Department` 表 | ✅ 暂不单独建表，由 `Region` 统一承载组织树 | 降低模型复杂度，后续如有需要可演进为 `OrgUnit/Department` 体系 |

---

## 九、与当前代码的对比

| 当前实现 | 方案二调整 |
|---|---|
| `UserToken.RoleId`（单激活角色） | 移除，改由 `PlatformType` 驱动多角色并集 |
| `Role.Platforms` 字符串 | 改为 `[Flags]` enum 或后续演进为 `RolePlatform` 关联表 |
| `User.DepartmentIds` 字符串 | 改为 `UserDepartment` 关联表 |
| `SwitchRole` API | 移除 |
| 权限缓存 key = `roleId` | 改为 `(userId, platformType)` |
| Claims 携带 Roles/Regions 等 | 简化 Claims，权限数据走 Redis 缓存 |
| `RequestAuthorizeFilter` 无权限校验 | 实现完整 API 级权限校验 |

---

## 十、RBAC 设计最佳实践清单（落地视角）

- **授权粒度**：  
  - 后端统一以 **API 粒度** 做权限控制；  
  - UI 按需在此基础上扩展按钮/字段级权限，但不反向影响后端鉴权。
- **角色设计**：  
  - 角色应尽量拆分为"可复用、单一职责"的组合单元（如"用户查看"与"用户编辑"拆分）；  
  - 禁止设计"只比管理员少一个权限"之类的隐式角色，避免后续收敛困难。
- **命名与编码**：  
  - `Role.Code`、`Menu.Code`、`ApiResource.Code` 必须是稳定、可读的业务编码；  
  - 禁止在业务代码中硬编码数据库自增 Id，所有引用应通过 Code 或枚举。
- **变更与审计**：  
  - 所有权限变更（角色-菜单、菜单-API、用户-角色）应记录审计日志，便于回溯与合规；  
  - 建议在管理后台保留"模拟某用户登录查看其权限"能力，用于排查问题。
- **性能与稳定性**：  
  - 鉴权失败路径同样要尽量轻量：权限缓存未命中时的 DB 查询逻辑需优化索引；  
  - 缓存重建失败时应采取**安全优先**策略（宁可 403 也不要放行），并在日志中显式记录。
