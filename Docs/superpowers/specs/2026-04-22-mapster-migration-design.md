# NexusStack 后端 Mapster 替换 AutoMapper 设计

> 状态：已实施
> 日期：2026-04-22  
> 范围：NexusStackBackend  
> 决策：使用 Mapster 替换 AutoMapper

## 背景

NexusStackBackend 当前使用 `AutoMapper 16.1.1`，而 AutoMapper 15.0 起已经进入需要 license key 的版本线。NexusStack 是开源企业中台模板，继续依赖 AutoMapper 会给模板使用者带来商业许可与部署配置负担，因此需要替换为更适合开源模板分发的对象映射方案。

当前项目中的 AutoMapper 不是孤立使用，而是位于两个关键层面：

- Core 层通过 `AutoMapperProfile`、`AutoMapperCreateProfile` 定义实体与 DTO 映射。
- EFCore 层通过 `AutoMapperRepository<TEntity, TKey>` 在通用仓储中使用 `ProjectTo<TProjectedType>()` 做 IQueryable 投影。
- 多个业务 Service 通过继承 `ServiceBase` 或直接注入 `IMapper` 使用 `Mapper.Map<T>()`。

因此替换方案必须同时覆盖“内存对象映射”和“EF Core 查询投影”两类场景。

## 目标

- 移除后端项目对 AutoMapper 的 NuGet 依赖。
- 使用开源许可友好的 Mapster 作为默认映射库。
- 保留现有通用仓储的 DTO 投影能力，避免把所有查询改成手写 `Select`。
- 尽量保持业务 Service 调用方式稳定，降低迁移风险。
- 将映射配置集中管理，保持模板用户易理解、易扩展。

## 非目标

- 本次不重构整个 Repository 架构。
- 本次不把所有映射改成手写 Mapper。
- 本次不引入 Mapperly 作为主方案，因为它更适合显式编译期 Mapper，而当前项目依赖泛型仓储投影。
- 本次不引入 Mapster code generation。可在后续性能优化阶段再评估。

## 方案选择

### 推荐方案：Mapster Runtime Config + DI

使用 Mapster 的 `TypeAdapterConfig` 替代 AutoMapper Profile，使用 Mapster DI `IMapper` 替代 AutoMapper `IMapper`，使用 `ProjectToType<T>()` 替代 `ProjectTo<T>()`。

优点：

- 迁移成本最低，最贴近现有 AutoMapper 架构。
- 可以保留泛型仓储中的 IQueryable 投影能力。
- 映射配置集中，适合模板项目。
- Mapster 使用 MIT License，适合开源项目分发。

代价：

- 仍然是运行时配置模型，不像 Mapperly 那样完全编译期生成。
- 某些 AutoMapper 的表达式配置需要逐项转换为 Mapster 语法。

### 备选方案：Mapperly

Mapperly 使用 source generator 生成映射代码，性能和可读性都很好，但需要为每组映射定义显式 mapper 方法。当前 NexusStack 的通用仓储使用泛型 `TProjectedType` 投影，直接迁移到 Mapperly 会迫使仓储层和服务层一起重构，短期成本过高。

### 备选方案：手写 Mapper

手写 Mapper 依赖最少、最透明，但会在模板中产生大量样板代码，也会削弱通用仓储投影能力。不适合作为当前主方案。

## 目标架构

### 包引用

Core 项目新增：

- `Mapster`
- `Mapster.DependencyInjection`

EFCore 项目新增：

- `Mapster`

Core 与 EFCore 项目移除：

- `AutoMapper`

### 命名与目录

为减少后续模板用户对 AutoMapper 的历史认知负担，代码命名应从 AutoMapper 专有名称迁出。

建议新增或重命名：

- `Domain/NexusStack.Core/MapProfiles/AutoMapperProfile.cs` → `Domain/NexusStack.Core/MapProfiles/MapsterProfile.cs`
- `Domain/NexusStack.Core/MapProfiles/AutoMapperCreateProfile.cs` → `Domain/NexusStack.Core/MapProfiles/MapsterCreateProfile.cs`
- `Domain/NexusStack.EFCore/Repository/AutoMapper/AutoMapperRepository.cs` → `Domain/NexusStack.EFCore/Repository/Mapping/MappingRepository.cs`
- `IAutoMapperRepository` → `IMappingRepository`
- `AddAllAutoMapper()` → `AddAllMapster()`

如果一次性重命名影响较大，可以先保留旧接口名并标记为兼容层，但最终模板应使用中性命名。

### 映射配置

使用 Mapster 的 `IRegister` 管理配置：

```csharp
public class MapsterProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Ignore(dest => dest.Roles)
            .Ignore(dest => dest.UserRoles)
            .Ignore(dest => dest.Departments)
            .Map(dest => dest.HasPassword, src => !string.IsNullOrWhiteSpace(src.Password));
    }
}
```

新增/编辑 DTO 到实体的映射保持独立配置：

```csharp
public class MapsterCreateProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateMenuDto, Menu>();
        config.NewConfig<CreateRoleDto, Role>();
        config.NewConfig<CreateRegionDto, Region>();
        config.NewConfig<CreateUserDto, User>()
            .Ignore(dest => dest.UserRoles);
    }
}
```

### DI 注册

`AddAllMapster()` 负责：

- 创建或使用 `TypeAdapterConfig.GlobalSettings`
- 扫描当前加载程序集中的 `IRegister` 实现
- 调用 `config.Scan(assemblies)`
- 注册 `TypeAdapterConfig`
- 注册 Mapster DI `ServiceMapper`

示意：

```csharp
public static IServiceCollection AddAllMapster(this IServiceCollection services)
{
    var config = TypeAdapterConfig.GlobalSettings;
    var assemblies = AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => a.GetName().Name?.StartsWith("NexusStack.") == true)
        .ToArray();

    config.Scan(assemblies);

    services.AddSingleton(config);
    services.AddScoped<MapsterMapper.IMapper, ServiceMapper>();

    return services;
}
```

具体扫描逻辑应优先复用项目现有 `TypeFinders` 风格，避免引入另一套反射策略。

### 仓储投影

`AutoMapperRepository` 当前使用：

```csharp
query.ProjectTo<TProjectedType>(MapperConfig)
```

迁移后使用：

```csharp
query.ProjectToType<TProjectedType>(MapperConfig)
```

其中 `MapperConfig` 类型从 AutoMapper 的 `IConfigurationProvider` 改为 Mapster 的 `TypeAdapterConfig`。

构造函数从：

```csharp
protected readonly IMapper Mapper;
protected readonly IConfigurationProvider MapperConfig;
```

调整为：

```csharp
protected readonly MapsterMapper.IMapper Mapper;
protected readonly TypeAdapterConfig MapperConfig;
```

### 服务层对象映射

业务层中现有：

```csharp
Mapper.Map<TDestination>(source)
Mapper.Map(source, destination)
```

Mapster DI 的 `IMapper` 支持类似调用方式。迁移时优先保持服务层调用形态不变，只替换命名空间与注入类型。

如遇到 API 差异，以局部适配为准，不在第一阶段批量改成 `source.Adapt<T>()`，避免扩散静态映射调用。

## 迁移步骤

1. 修改 NuGet 引用，移除 AutoMapper，加入 Mapster 与 Mapster.DependencyInjection。
2. 新增 Mapster 配置类，将 `AutoMapperProfile` 与 `AutoMapperCreateProfile` 中的映射逐条迁移。
3. 替换 `ServiceCollectionExtensions.AddAllAutoMapper()` 为 `AddAllMapster()`，并更新启动注册调用。
4. 替换 EFCore 通用仓储中的 AutoMapper 类型、命名空间与 `ProjectTo` 调用。
5. 替换 Core 层服务、Controller 基类、HostedService 中的 `AutoMapper.IMapper` 为 `MapsterMapper.IMapper`。
6. 移除或重命名 AutoMapper 专有目录与接口名，确保模板表意清晰。
7. 构建并修复编译错误。
8. 针对关键映射路径做回归验证。

## 重点验证场景

- 用户列表：`User -> UserDto`，确认 `HasPassword`、`UserRoles`、`Departments` 行为不回归。
- 当前用户：`User -> CurrentUserDto`。
- 用户角色：`UserRole -> UserRoleDto`，确认 `RoleName`、`Platforms` 映射正确。
- Token：`UserToken -> UserTokenDto`、`UserTokenCacheDto`、`UserTokenLogDto`。
- 菜单树：`Menu -> MenuDto`、`Menu -> MenuTreeDto`，确认 `Children` 忽略后由业务逻辑组树。
- 区域树：`Region -> RegionDto`、`Region -> RegionTreeDto`，确认 `Children` 忽略后由业务逻辑组树。
- 权限配置：`Permission -> RolePermissionDto`。
- 创建/编辑：`CreateMenuDto -> Menu`、`CreateRoleDto -> Role`、`CreateRegionDto -> Region`、`CreateUserDto -> User`。
- 通用仓储分页投影：`GetPagedListAsync<TProjectedType>()` 正常生成 SQL 并返回 DTO。

## 风险与处理

### IQueryable 投影兼容风险

AutoMapper 与 Mapster 的表达式翻译能力不是完全等价的。迁移后必须重点验证仓储中的 `ProjectToType<T>()` 是否能被 EF Core 正确翻译。

处理方式：

- 优先保持映射表达式简单。
- 对 EF Core 无法翻译的复杂映射，改为服务层手动补充字段。
- 对少数复杂查询可以保留手写 `Select`，但不作为默认迁移路径。

### 命名冲突风险

`AutoMapper.IMapper` 与 `MapsterMapper.IMapper` 类型名相同，迁移期间容易误引用。

处理方式：

- 全量移除 AutoMapper 包后让编译器暴露残留引用。
- 对必要位置使用完整命名空间 `MapsterMapper.IMapper`。

### 模板用户破坏性变更

如果模板用户已经基于 `IAutoMapperRepository` 继承扩展，接口改名会产生破坏性变更。

处理方式：

- 当前 NexusStack 尚处模板演进阶段，建议直接改为中性命名。
- 如需要兼容，可保留 `IAutoMapperRepository` 作为 `[Obsolete]` 继承接口，后续版本删除。

## 完成标准

- 后端项目中不存在 `AutoMapper` 包引用。
- 后端源码中不存在 `using AutoMapper;`、`ProjectTo<`、`CreateMap(`、`Profile` 等 AutoMapper 专有用法。
- `dotnet build` 通过。
- 至少完成核心 WebAPI 项目启动验证。
- RBAC 相关基础接口可正常返回 DTO，包括用户、角色、菜单、区域、权限树。

## 后续可选优化

- 评估 Mapster code generation，用于高频映射或大型 DTO。
- 为关键映射添加单元测试，覆盖自定义字段和 Ignore 行为。
- 将 `MapProfiles` 目录改名为 `Mappings`，进一步去 AutoMapper 历史命名。
