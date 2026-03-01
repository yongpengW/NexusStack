# NexusStack RBAC 权限系统设计文档

> 文档状态：**V1 实现已完成**  
> 最后更新：2026-03  
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

> ✅ **已解决**：`UserToken.RoleId` 字段已在实现中完全移除，Token 仅绑定 `UserId + PlatformType`，
> 权限由 `(UserId, PlatformType)` 对应的 Redis 缓存驱动，不再依赖单一激活角色。

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
RequestAuthenticationTokenHandler（身份认证）
  ├─ 读 Authorization Header → ValidateTokenAsync (Redis → DB fallback)
  ├─ Token 无效 → AuthenticateResult.Fail
  ├─ GetOrSetAsync(UserId, PlatformType) → 拉取/构建 UserContextCacheDto
  └─ 写入 HttpContext.Items["NexusStack.UserContext"]
  ↓
RequestAuthorizeFilter（权限校验，纯内存，不查 DB）
  ├─ [AllowAnonymous] → 直接放行
  ├─ OpenAPI 专属 Scheme → 只验身份，放行
  ├─ IsAuthenticated != true → 401 请先登录
  ├─ Items 中无 UserContextCacheDto → 401 用户上下文缺失
  ├─ userContext.IsEnable == false → 403 用户已禁用
  ├─ apiKey = RouteTemplate.ToLower() + ":" + HttpMethod.ToUpper()
  │         （例：api/role/permission/{roleId}:POST）
  └─ ApiPermissionKeys.Contains(apiKey) → 否 → 403 暂无权限 / 是 → 放行
```

> **关键设计**：`RequestAuthorizeFilter` 不注入任何 Service，完全依赖
> `RequestAuthenticationTokenHandler` 预写入 `HttpContext.Items` 的 `UserContextCacheDto`，
> 鉴权热路径为纯内存操作，O(1) 时间复杂度。

### 5.3 权限缓存结构（已落地实现）

```csharp
// Redis Key: "{CoreRedisConstants.UserContext.Format(userId, (int)platformType)}:v2"
// 版本后缀 :v2 用于在 Key 格式升级时自动淘汰旧缓存，无需手动清空 Redis。

// Value: UserContextCacheDto（JSON 序列化后存储）
public class UserContextCacheDto
{
    public string          UserName          { get; set; }  // 用于禁用提示等
    public string          Email             { get; set; }
    public bool            IsEnable          { get; set; }  // 鉴权时直接判断，无需查 DB
    public List<long>      RoleIds           { get; set; }  // 当前平台下的角色集合（并集）
    public List<long>      RegionIds         { get; set; }  // 组织/地区 Id 列表
    public HashSet<string> ApiPermissionKeys { get; set; }  // 预计算 API 白名单
    // ApiPermissionKeys 格式："routetemplate:HTTPMETHOD"（全小写路由:大写方法）
    // 示例：{ "api/role:GET", "api/role:POST", "api/role/permission/{roleId}:POST", ... }
}

// 构建逻辑（UserContextCacheService.BuildFromDbAsync）：
var user = dbContext.Set<User>()
    .Where(u => u.Id == userId)
    .Select(u => new { u.UserName, u.Email, u.IsEnable })
    .FirstOrDefault();

// 平台过滤 + 仅取启用角色（r.IsEnable 为 true 且 r.Platforms 包含 platformType）
var roleIds = userRoleService.GetUserRoles(userId, platformType)
    .Select(ur => ur.RoleId).Distinct().ToList();

var regionIds = dbContext.Set<UserDepartment>()
    .Where(ud => ud.UserId == userId)
    .Select(ud => ud.DepartmentId).ToList();

// 多角色并集：Role → Permission → Menu → MenuResource → ApiResource
var menuIds = dbContext.Set<Permission>()
    .Where(p => roleIds.Contains(p.RoleId))
    .Select(p => p.MenuId).Distinct().ToList();

var apiPermissionKeys = (
    from mr in dbContext.Set<MenuResource>()
    join ar in dbContext.Set<ApiResource>() on mr.ApiResourceId equals ar.Id
    where menuIds.Contains(mr.MenuId)
          && ar.RoutePattern != null && ar.RequestMethod != null
    select ar.RoutePattern.ToLower() + ":" + ar.RequestMethod.ToUpper()
).Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);
```

#### 5.3.1 权限缓存 Key 格式说明

- **Key 维度**：`(UserId, PlatformType)`，不直接以 `RoleId` 为 Key，原因：  
  - 平台上下文会影响可用角色集合；  
  - 用户多角色并集后的结果远比单一角色更贴近真实权限视角。  
- **API 权限 Key 格式（`routetemplate:METHOD`）**：  
  - 使用 **路由模板** 而非 `ControllerName:ActionName`，可正确区分同控制器内同名重载 Action；  
  - 例：`RoleController` 同时有 `POST api/role`（新增角色）和 `POST api/role/permission/{roleId}`（修改权限），旧格式均为 `Role:PostAsync:POST`（冲突），新格式为 `api/role:POST` 与 `api/role/permission/{roleId}:POST`（唯一）；  
  - 与 `InitApiResourceService` 写入 `ApiResource.Code` 的格式、`RequestAuthorizeFilter` 构造 `apiKey` 的逻辑三方保持完全一致。  
- **缓存版本控制**：Key 末尾附加 `:v2`，后续如 Key 格式再次升级，只需改版本号即可自动淘汰全量旧缓存。  
- **Value 内容扩展建议**：  
  - 后续如需支持前端按钮/字段级权限，可在 `UserContextCacheDto` 中新增 `HashSet<string> UiPermissions`，由前端约定编码（如 `user.create`）；  
  - 后端鉴权始终以 API 粒度为准，UI 权限仅作展示/交互约束。

### 5.4 缓存失效矩阵（已落地实现）

所有失效操作均调用 `IUserContextCacheService.InvalidateAsync(userId)`，精准删除对应 Redis Key，避免全量清除。

| 触发操作 | 失效范围 | 实现位置 |
|---|---|---|
| 修改角色权限（菜单-角色关联变更） | 该角色所有用户，全平台 | `PermissionService.ChangeRolePermissionAsync` |
| 修改角色属性（含 `Platforms`/`IsEnable`） | 该角色所有用户，全平台 | `RoleController.PutAsync` |
| 修改用户角色或组织绑定 | 该用户，全平台 | `UserController.PutAsync` |
| 用户启用 | 该用户，全平台 | `UserController.EnableAsync` |
| 用户禁用 | 该用户，全平台 | `UserController.DisableAsync` |
| 用户删除 | 该用户，全平台（删除前执行） | `UserController.DeleteAsync` |
| 修改菜单属性（`IsVisible`/`PlatformType` 等） | 持有该菜单权限的所有用户 | `MenuController.PutAsync` |
| 删除菜单 | 持有该菜单权限的所有用户（**删除前**查询，防级联清除后无法溯源） | `MenuController.DeleteAsync` |
| 菜单绑定/解绑 API 资源 | 持有该菜单权限的所有用户 | `MenuController.BindResourceAsync` |
| 退出登录 | 该用户，当前平台（精准单平台） | `TokenController.SignoutAsync` |

> **受影响用户查询链**（用于菜单类操作）：  
> `menuId → Permission.RoleId → UserRole.UserId`，再对每个 `userId` 调用 `InvalidateAsync`。

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

## 七、ICurrentUser 接口（已落地实现）

基于方案二，`ICurrentUser` 仅从 Claims 读取身份标识，**权限相关数据（角色、组织）统一从 `HttpContext.Items` 中的 `UserContextCacheDto` 读取**：

```csharp
public interface ICurrentUser
{
    long   UserId          { get; }   // 从 Claim 读取
    string UserName        { get; }   // 从 Claim 读取
    string Email           { get; }   // 从 Claim 读取
    bool   IsAuthenticated { get; }   // 从 ClaimsPrincipal.Identity 读取
    string Token           { get; }   // 从 Claim 读取
    long   TokenId         { get; }   // 从 Claim 读取
    int    PlatformType    { get; }   // 从 Claim 读取（存储为整数字符串，如 "1" 对应 Admin）

    // 以下从 HttpContext.Items["NexusStack.UserContext"] 读取（不走 Claims / DB）
    IReadOnlyList<long> RoleIds    { get; }   // 当前平台下的有效角色 Id 列表
    IReadOnlyList<long> RegionIds  { get; }   // 用户所属组织/地区 Id 列表
}
```

**Claims 与 HttpContext.Items 分工**：

| 数据 | 存储位置 | 写入时机 |
|---|---|---|
| UserId / TokenId / PlatformType / UserName / Email | JWT Claims | `RequestAuthenticationTokenHandler` 认证成功后 |
| RoleIds / RegionIds / IsEnable / ApiPermissionKeys | `HttpContext.Items["NexusStack.UserContext"]` | 同上，写入 `UserContextCacheDto`（来自 Redis 缓存） |

> `PlatformType` Claim 存储为**整数字符串**（如 `"1"` 对应 `PlatformType.Admin`），
> `CurrentUser.PlatformType` 通过 `FindClaimValue<int>()` 解析，
> 与 `((int)userToken.PlatformType).ToString()` 的写入格式完全匹配。

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

## 九、设计目标与落地状态对比

| 设计目标 | 落地状态 | 说明 |
|---|---|---|
| 移除 `UserToken.RoleId`（单激活角色） | ✅ 已完成 | Token 仅绑定 `UserId + PlatformType` |
| `Role.Platforms` 改为 `[Flags]` enum | ✅ 已完成 | 使用位运算过滤 `(r.Platforms & platformType) != 0` |
| `User.DepartmentIds` 改为 `UserDepartment` 关联表 | ✅ 已完成 | `UserDepartment(UserId, DepartmentId)` |
| 移除 `SwitchRole` API | ✅ 已完成 | 权限始终为多角色并集，无切换概念 |
| 权限缓存 Key 改为 `(UserId, PlatformType)` | ✅ 已完成 | 附加版本号 `:v2`，支持平滑升级 |
| Claims 简化，权限数据走 Redis 缓存 | ✅ 已完成 | `RoleIds`/`RegionIds` 从 `HttpContext.Items` 读取 |
| `RequestAuthorizeFilter` 实现完整 API 级权限校验 | ✅ 已完成 | 纯内存 O(1) 校验，不访问 DB |
| 登录时校验平台角色存在 | ✅ 已完成 | `LoginWithPasswordAsync` 中验密后立即校验 |
| 禁用角色不参与权限计算 | ✅ 已完成 | `UserRoleService.GetUserRoles` 过滤 `r.IsEnable` |
| 禁用用户立即生效 | ✅ 已完成 | `IsEnable` 写入缓存，禁用时触发缓存失效 |
| 全链路缓存失效覆盖 | ✅ 已完成 | 见第 5.4 节失效矩阵 |
| API 资源自动注册 | ✅ 已完成 | `InitApiResourceService` 启动时扫描路由，`Code = RouteTemplate:Method` |
| 解决同名 Action 冲突 | ✅ 已完成 | `Code` 使用路由模板而非 ActionName，全局唯一 |
| 移除 SSO 遗留集成 | ✅ 已完成 | `UserController.DeleteAsync` 中的 SSO HTTP 调用已删除 |

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

---

## 十一、实现落地说明

### 11.1 核心组件一览

| 组件 | 职责 | 文件路径 |
|---|---|---|
| `RequestAuthenticationTokenHandler` | 认证：验 Token → 构建/读取缓存 → 写 `HttpContext.Items` | `Domain/NexusStack.Core/Authentication/` |
| `RequestAuthorizeFilter` | 授权：从 `Items` 读 `UserContextCacheDto`，O(1) 校验 | `Domain/NexusStack.Core/Filters/` |
| `UserContextCacheService` | 缓存构建/读取/失效，Key = `usercontext:{userId}:{platform}:v2` | `Domain/NexusStack.Core/Services/Users/` |
| `UserContextCacheDto` | 缓存数据结构：`IsEnable` + `RoleIds` + `RegionIds` + `ApiPermissionKeys` | `Domain/NexusStack.Core/Dtos/Users/` |
| `InitApiResourceService` | 启动时扫描路由，`InsertOrUpdate` `ApiResource`，`Code = RouteTemplate:Method` | `Domain/NexusStack.Core/HostedServices/` |
| `UserRoleService.GetUserRoles` | 平台过滤 + 仅返回 `r.IsEnable = true` 的角色 | `Domain/NexusStack.Core/Services/Users/` |
| `UserTokenService` | 登录：验密 → 判禁用 → 判平台角色 → 生成 Token → 预热缓存 | `Domain/NexusStack.Core/Services/Users/` |

### 11.2 ApiResource.Code 唯一性策略

```
旧格式（已废弃）：{Namespace}.{ControllerName}.{ActionName}
  → 同控制器同名重载 Action（如两个 PostAsync）Code 相同，导致 DB 覆盖和鉴权 Key 冲突。

新格式（当前实现）：{RoutePattern?.ToLowerInvariant()}:{RequestMethod}
  示例：api/role:POST          ← RoleController.PostAsync（新增角色）
        api/role/permission/{roleId}:POST  ← RoleController.PostAsync（修改角色权限）
  → 基于路由模板全局唯一，彻底解决同名重载冲突。
```

三方格式对齐（必须保持一致）：

| 环节 | Key 构造 | 位置 |
|---|---|---|
| 写入 `ApiResource.Code` | `RoutePattern?.ToLowerInvariant():RequestMethod` | `InitApiResourceService` |
| 构建 `ApiPermissionKeys` | `ar.RoutePattern.ToLower():ar.RequestMethod.ToUpper()` | `UserContextCacheService.BuildFromDbAsync` |
| 鉴权时构造 `apiKey` | `Template.ToLowerInvariant():Request.Method.ToUpperInvariant()` | `RequestAuthorizeFilter` |

### 11.3 缓存版本管理

当 `UserContextCacheDto` 结构或 `ApiPermissionKeys` Key 格式发生变更时，只需修改 `UserContextCacheService` 中的常量：

```csharp
private const string CacheVersion = "v2";  // 升级此值即可自动淘汰全量旧缓存
```

旧版本的 Redis Key 不再被读取，自然过期（TTL = 10h），无需手动执行 `FLUSHDB`。

### 11.4 登录前置校验顺序

```
1. 查找用户（用户名 / 手机号）
2. 验证密码
3. 校验 IsEnable（账号是否被禁用）       ← 先于平台角色校验，避免给禁用用户暴露角色信息
4. 校验当前平台下是否有启用角色          ← GetUserRoles 已过滤 r.IsEnable
5. GenerateUserTokenAsync → 预热 UserContextCacheDto
```

### 11.5 已解决的关键问题清单

| # | 问题 | 修复位置 |
|---|---|---|
| 1 | `PostAsync` 创建用户时 `UserId = 0` 被写入 `UserRole` | `UserController.PostAsync`：先 Insert User 再绑定角色 |
| 2 | `GetListAsync` 使用 INNER JOIN 导致无角色用户被排除 | 改为子查询 `Contains` |
| 3 | `RequestAuthorizeFilter` 每次查 DB，鉴权热路径性能低 | 改为纯读缓存，O(1) |
| 4 | 权限变更后缓存未失效（Role/Menu/Permission 各操作） | 全链路补充 `InvalidateAsync` 调用 |
| 5 | `PlatformType` Claim 存为枚举名字符串，解析时 `FormatException` | 改为 `((int)PlatformType).ToString()` |
| 6 | 禁用角色仍参与权限计算 | `UserRoleService.GetUserRoles` 增加 `r.IsEnable` 过滤 |
| 7 | `RoleController.PutAsync` 中 AutoMapper 绕过 `IsSystem` 保护 | Mapper.Map 前增加前置 `IsSystem` 检查 |
| 8 | 同名重载 Action 的 `ApiResource.Code` 冲突 | Code 改为 `RouteTemplate:Method` 格式 |
| 9 | `UserController.PutAsync` catch 块吞掉原始异常类型 | 改为 `catch { throw; }` |
| 10 | 登录禁用用户返回"无角色"而非"已禁用"的语义错误 | 密码验证后立即检查 `IsEnable`，优先于平台角色校验 |
