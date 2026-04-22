# Mapster Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace AutoMapper with Mapster in NexusStackBackend while preserving object mapping, EF Core projection, and existing service behavior.

**Architecture:** Use Mapster runtime configuration through `IRegister` classes in Core, register `TypeAdapterConfig` and `MapsterMapper.IMapper` through DI, and replace repository `ProjectTo<T>()` calls with Mapster `ProjectToType<T>()`. Keep service-level mapper calls shaped like the current `Mapper.Map<T>()` usage to minimize business-code churn.

**Tech Stack:** .NET 10, EF Core 10, Mapster, Mapster.DependencyInjection, X.PagedList, Ardalis.Specification, LinqKit.

---

## File Structure

- Modify `Domain/NexusStack.Core/NexusStack.Core.csproj`: remove AutoMapper package, add Mapster packages.
- Modify `Domain/NexusStack.EFCore/NexusStack.EFCore.csproj`: remove AutoMapper package, add Mapster package.
- Rename `Domain/NexusStack.Core/MapProfiles/AutoMapperProfile.cs` to `Domain/NexusStack.Core/MapProfiles/MapsterProfile.cs`: convert entity-to-DTO maps.
- Rename `Domain/NexusStack.Core/MapProfiles/AutoMapperCreateProfile.cs` to `Domain/NexusStack.Core/MapProfiles/MapsterCreateProfile.cs`: convert create/edit DTO-to-entity maps.
- Rename `Domain/NexusStack.EFCore/Repository/AutoMapper/IAutoMapperRepository.cs` to `Domain/NexusStack.EFCore/Repository/Mapping/IMappingRepository.cs`: use neutral repository naming.
- Rename `Domain/NexusStack.EFCore/Repository/AutoMapper/AutoMapperRepository.cs` to `Domain/NexusStack.EFCore/Repository/Mapping/MappingRepository.cs`: replace AutoMapper projection with Mapster projection.
- Modify `Domain/NexusStack.EFCore/Repository/IServiceBase.cs`: inherit `IMappingRepository`.
- Modify `Domain/NexusStack.EFCore/Repository/ServiceBase.cs`: inherit `MappingRepository` and inject `MapsterMapper.IMapper`.
- Modify `Domain/NexusStack.Core/ServiceCollectionExtensions.cs`: replace AutoMapper scanning and registration with Mapster registration.
- Modify all Core services, hosted services, schedules, and controllers that import `AutoMapper` or inject `IMapper`: switch to `MapsterMapper.IMapper`.

---

### Task 1: Package References

**Files:**
- Modify: `Domain/NexusStack.Core/NexusStack.Core.csproj`
- Modify: `Domain/NexusStack.EFCore/NexusStack.EFCore.csproj`

- [ ] **Step 1: Update Core package references**

In `Domain/NexusStack.Core/NexusStack.Core.csproj`, replace:

```xml
<PackageReference Include="AutoMapper" Version="16.1.1" />
```

with:

```xml
<PackageReference Include="Mapster" Version="7.4.0" />
<PackageReference Include="Mapster.DependencyInjection" Version="1.0.1" />
```

- [ ] **Step 2: Update EFCore package references**

In `Domain/NexusStack.EFCore/NexusStack.EFCore.csproj`, replace:

```xml
<PackageReference Include="AutoMapper" Version="16.1.1" />
```

with:

```xml
<PackageReference Include="Mapster" Version="7.4.0" />
```

- [ ] **Step 3: Restore packages**

Run:

```powershell
dotnet restore NexusStack.sln
```

Expected: restore completes without AutoMapper package references in Core or EFCore.

- [ ] **Step 4: Commit package migration**

Run:

```powershell
git add Domain/NexusStack.Core/NexusStack.Core.csproj Domain/NexusStack.EFCore/NexusStack.EFCore.csproj
git commit -m "build: replace AutoMapper packages with Mapster"
```

---

### Task 2: Mapster Mapping Profiles

**Files:**
- Move: `Domain/NexusStack.Core/MapProfiles/AutoMapperProfile.cs` to `Domain/NexusStack.Core/MapProfiles/MapsterProfile.cs`
- Move: `Domain/NexusStack.Core/MapProfiles/AutoMapperCreateProfile.cs` to `Domain/NexusStack.Core/MapProfiles/MapsterCreateProfile.cs`

- [ ] **Step 1: Convert entity-to-DTO mapping profile**

Replace the contents of `Domain/NexusStack.Core/MapProfiles/MapsterProfile.cs` with:

```csharp
using Mapster;
using NexusStack.Core.Dtos;
using NexusStack.Core.Dtos.DownloadCenter;
using NexusStack.Core.Dtos.Files;
using NexusStack.Core.Dtos.GlobalSettings;
using NexusStack.Core.Dtos.Menus;
using NexusStack.Core.Dtos.Regions;
using NexusStack.Core.Dtos.Roles;
using NexusStack.Core.Dtos.ScheduleTasks;
using NexusStack.Core.Dtos.Users;
using NexusStack.Core.Entities.AsyncTasks;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Entities.Users;
using NexusStack.Infrastructure.Enums;
using File = NexusStack.Core.Entities.SystemManagement.File;

namespace NexusStack.Core.MapProfiles
{
    public class MapsterProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<User, UserDto>()
                .Ignore(a => a.Roles)
                .Ignore(a => a.UserRoles)
                .Ignore(a => a.Departments)
                .Map(a => a.HasPassword, c => !string.IsNullOrWhiteSpace(c.Password));

            config.NewConfig<UserDepartment, UserDepartmentDto>();
            config.NewConfig<Role, RoleDto>();
            config.NewConfig<UserRole, UserRoleDto>()
                .Map(a => a.RoleName, c => c.Role != null ? c.Role.Name : string.Empty)
                .Map(a => a.Platforms, c => c.Role != null ? c.Role.Platforms : PlatformType.All);
            config.NewConfig<UserToken, UserTokenDto>()
                .Map(a => a.UserName, c => c.User != null ? c.User.UserName : string.Empty);
            config.NewConfig<UserTokenCacheDto, UserTokenDto>();
            config.NewConfig<Permission, RolePermissionDto>();
            config.NewConfig<User, CurrentUserDto>()
                .Map(a => a.HasPassword, c => !string.IsNullOrWhiteSpace(c.Password));
            config.NewConfig<UserToken, UserTokenCacheDto>()
                .Map(a => a.UserName, c => c.User != null ? c.User.UserName : string.Empty);
            config.NewConfig<UserToken, UserTokenLogDto>()
                .Map(a => a.loginUser, c => c.User != null ? (c.User.RealName ?? c.User.UserName) : string.Empty)
                .Map(a => a.loginAt, c => c.CreatedAt);

            config.NewConfig<Menu, MenuDto>();
            config.NewConfig<Menu, MenuTreeDto>().Ignore(a => a.Children);
            config.NewConfig<MenuDto, MenuTreeDto>();
            config.NewConfig<SeedDataTask, SeedDataTaskDto>();
            config.NewConfig<ApiResource, ApiResourceDto>();
            config.NewConfig<ScheduleTask, ScheduleTaskDto>();
            config.NewConfig<ApiResourceDto, MenuResourceDto>();
            config.NewConfig<File, FileDto>();
            config.NewConfig<Region, RegionDto>();
            config.NewConfig<Region, RegionTreeDto>().Ignore(a => a.Children);
            config.NewConfig<RegionDto, RegionTreeDto>();
            config.NewConfig<ScheduleTask, ScheduleTaskExecuteDto>();
            config.NewConfig<ScheduleTaskExecuteDto, ScheduleTask>();
            config.NewConfig<AsyncTask, AsyncTaskDto>();
            config.NewConfig<File, ExportFileDto>();
            config.NewConfig<GlobalSettings, GlobalSettingDto>();
            config.NewConfig<DownloadItem, DownloadItemDto>();
        }
    }
}
```

- [ ] **Step 2: Convert create/edit mapping profile**

Replace the contents of `Domain/NexusStack.Core/MapProfiles/MapsterCreateProfile.cs` with:

```csharp
using Mapster;
using NexusStack.Core.Dtos.GlobalSettings;
using NexusStack.Core.Dtos.Menus;
using NexusStack.Core.Dtos.Regions;
using NexusStack.Core.Dtos.Roles;
using NexusStack.Core.Dtos.Users;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Entities.Users;

namespace NexusStack.Core.MapProfiles
{
    public class MapsterCreateProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateMenuDto, Menu>();
            config.NewConfig<CreateRoleDto, Role>();
            config.NewConfig<CreateRegionDto, Region>();
            config.NewConfig<CreateUserDto, User>()
                .Ignore(a => a.UserRoles);
            config.NewConfig<CreateUserRoleDto, UserRole>();
            config.NewConfig<CreateGlobalSettingDto, GlobalSettings>();
        }
    }
}
```

- [ ] **Step 3: Verify old profile files are gone**

Run:

```powershell
Test-Path Domain/NexusStack.Core/MapProfiles/AutoMapperProfile.cs
Test-Path Domain/NexusStack.Core/MapProfiles/AutoMapperCreateProfile.cs
```

Expected: both commands print `False`.

- [ ] **Step 4: Commit mapping profile migration**

Run:

```powershell
git add Domain/NexusStack.Core/MapProfiles
git commit -m "refactor: migrate mapping profiles to Mapster"
```

---

### Task 3: Mapping Repository Projection

**Files:**
- Move: `Domain/NexusStack.EFCore/Repository/AutoMapper/IAutoMapperRepository.cs` to `Domain/NexusStack.EFCore/Repository/Mapping/IMappingRepository.cs`
- Move: `Domain/NexusStack.EFCore/Repository/AutoMapper/AutoMapperRepository.cs` to `Domain/NexusStack.EFCore/Repository/Mapping/MappingRepository.cs`
- Modify: `Domain/NexusStack.EFCore/Repository/IServiceBase.cs`
- Modify: `Domain/NexusStack.EFCore/Repository/ServiceBase.cs`

- [ ] **Step 1: Convert repository interface namespace and name**

In `Domain/NexusStack.EFCore/Repository/Mapping/IMappingRepository.cs`, change the namespace and interface declaration to:

```csharp
namespace NexusStack.EFCore.Repository.Mapping
{
    public interface IMappingRepository<TEntity, TKey> : IRepositoryBase<TEntity, TKey> where TEntity : class
```

Keep the existing method signatures unchanged.

- [ ] **Step 2: Convert repository implementation imports**

At the top of `Domain/NexusStack.EFCore/Repository/Mapping/MappingRepository.cs`, replace AutoMapper imports with:

```csharp
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using LinqKit;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository.Base;
using System.Globalization;
using System.Linq.Expressions;
using X.PagedList;
using X.PagedList.EF;
```

- [ ] **Step 3: Convert repository class and constructor**

In `MappingRepository.cs`, replace the class declaration and fields with:

```csharp
public abstract class MappingRepository<TEntity, TKey> : RepositoryBase<TEntity, TKey>, IMappingRepository<TEntity, TKey> where TEntity : class
{
    protected readonly IMapper Mapper;
    protected readonly TypeAdapterConfig MapperConfig;

    protected MappingRepository(MainContext dbContext, IMapper mapper, TypeAdapterConfig mapperConfig)
        : this(dbContext, mapper, mapperConfig, SpecificationEvaluator.Default)
    {
    }

    public MappingRepository(MainContext dbContext, IMapper mapper, TypeAdapterConfig mapperConfig, ISpecificationEvaluator specificationEvaluator)
        : base(dbContext, specificationEvaluator)
    {
        Mapper = mapper;
        MapperConfig = mapperConfig;
    }
```

- [ ] **Step 4: Replace projection calls**

In `MappingRepository.cs`, replace every:

```csharp
.ProjectTo<TProjectedType>(MapperConfig)
```

with:

```csharp
.ProjectToType<TProjectedType>(MapperConfig)
```

- [ ] **Step 5: Update service base interface**

In `Domain/NexusStack.EFCore/Repository/IServiceBase.cs`, replace:

```csharp
using NexusStack.EFCore.Repository.AutoMapper;
```

with:

```csharp
using NexusStack.EFCore.Repository.Mapping;
```

and replace:

```csharp
public interface IServiceBase<TEntity, TKey> : IAutoMapperRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
```

with:

```csharp
public interface IServiceBase<TEntity, TKey> : IMappingRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
```

- [ ] **Step 6: Update service base implementation**

In `Domain/NexusStack.EFCore/Repository/ServiceBase.cs`, replace the AutoMapper import and base class with:

```csharp
using Mapster;
using MapsterMapper;
using NexusStack.EFCore.Repository.Mapping;
```

and:

```csharp
public partial class ServiceBase<TEntity, TKey> : MappingRepository<TEntity, TKey>, IServiceBase<TEntity, TKey> where TEntity : class, IEntity<TKey>
```

Change both constructors to include `TypeAdapterConfig mapperConfig`:

```csharp
public ServiceBase(MainContext dbContext, IMapper mapper, TypeAdapterConfig mapperConfig)
    : base(dbContext, mapper, mapperConfig)
{
}

public ServiceBase(MainContext dbContext, IMapper mapper, TypeAdapterConfig mapperConfig)
    : base(dbContext, mapper, mapperConfig)
{
}
```

The second constructor is for `ServiceBase<TEntity>`.

- [ ] **Step 7: Commit repository migration**

Run:

```powershell
git add Domain/NexusStack.EFCore/Repository
git commit -m "refactor: replace AutoMapper repository projection with Mapster"
```

---

### Task 4: DI Registration

**Files:**
- Modify: `Domain/NexusStack.Core/ServiceCollectionExtensions.cs`

- [ ] **Step 1: Replace AutoMapper imports**

In `ServiceCollectionExtensions.cs`, remove:

```csharp
using AutoMapper;
```

Add:

```csharp
using Mapster;
using MapsterMapper;
```

- [ ] **Step 2: Replace startup registration call**

Replace:

```csharp
builder.Services.AddAllAutoMapper();
```

with:

```csharp
builder.Services.AddAllMapster();
```

- [ ] **Step 3: Replace registration method**

Replace the whole `AddAllAutoMapper` method with:

```csharp
/// <summary>
/// 注册所有 Mapster 映射配置
/// </summary>
/// <param name="services"></param>
/// <returns></returns>
public static IServiceCollection AddAllMapster(this IServiceCollection services)
{
    var registerTypes = TypeFinders.SearchTypes(typeof(IRegister), TypeFinders.TypeClassification.Class).ToArray();

    if (registerTypes.Length == 0)
    {
        Console.WriteLine("警告: 未找到任何 Mapster IRegister 类型");
    }

    var assemblies = registerTypes.Select(t => t.Assembly).Distinct().ToArray();
    var config = TypeAdapterConfig.GlobalSettings;

    if (assemblies.Length > 0)
    {
        config.Scan(assemblies);
    }

    Console.WriteLine($"注册 Mapster，共找到 {registerTypes.Length} 个 IRegister，分布在 {assemblies.Length} 个程序集中");
    foreach (var t in registerTypes) Console.WriteLine($"  找到 Mapster Register: {t.FullName}");

    services.AddSingleton(config);
    services.AddScoped<IMapper, ServiceMapper>();

    return services;
}
```

- [ ] **Step 4: Commit DI migration**

Run:

```powershell
git add Domain/NexusStack.Core/ServiceCollectionExtensions.cs
git commit -m "refactor: register Mapster mapping services"
```

---

### Task 5: Application Mapper Usages

**Files:**
- Modify all `.cs` files reported by:
  `Get-ChildItem -Path . -Recurse -Include *.cs -File | Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\|\\.git\\' } | Select-String -Pattern 'using AutoMapper;|IMapper\\b'`

- [ ] **Step 1: Replace mapper imports in application code**

For every Core/Host file that contains:

```csharp
using AutoMapper;
```

replace it with:

```csharp
using MapsterMapper;
```

Do not change `Mapper.Map<T>()` calls in this step.

- [ ] **Step 2: Fix ambiguous IMapper references**

If a file has another `IMapper` type in scope, use explicit constructor parameter type:

```csharp
public SomeService(MainContext dbContext, MapsterMapper.IMapper mapper, TypeAdapterConfig mapperConfig)
```

For service classes that inherit `ServiceBase<TEntity>`, also pass the new config argument:

```csharp
public SomeService(MainContext dbContext, IMapper mapper, TypeAdapterConfig mapperConfig)
    : base(dbContext, mapper, mapperConfig)
{
}
```

Add:

```csharp
using Mapster;
using MapsterMapper;
```

when the class needs both `IMapper` and `TypeAdapterConfig`.

- [ ] **Step 3: Keep service map calls stable**

Existing calls like:

```csharp
var dto = Mapper.Map<UserDto>(user);
Mapper.Map(input, entity);
```

should remain in that shape unless the compiler reports a Mapster API mismatch. If a two-argument map does not compile, replace it with:

```csharp
Mapper.Map(input, entity);
```

Expected: MapsterMapper supports this shape.

- [ ] **Step 4: Commit application mapper import migration**

Run:

```powershell
git add Domain Host BackgroundServices
git commit -m "refactor: switch services to Mapster mapper"
```

---

### Task 6: AutoMapper Residue Cleanup

**Files:**
- Modify any files found by the residue scans below.

- [ ] **Step 1: Search for AutoMapper residues**

Run:

```powershell
Get-ChildItem -Path . -Recurse -Include *.cs,*.csproj -File |
  Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\|\\.git\\' } |
  Select-String -Pattern 'AutoMapper|ProjectTo<|CreateMap\\(|Profile\\b|AddAutoMapper'
```

Expected: no matches in source or project files. Matches in docs are acceptable only in migration design/plan documents.

- [ ] **Step 2: Search for old repository namespace**

Run:

```powershell
Get-ChildItem -Path . -Recurse -Include *.cs -File |
  Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\|\\.git\\' } |
  Select-String -Pattern 'Repository\\.AutoMapper|IAutoMapperRepository|AutoMapperRepository'
```

Expected: no matches.

- [ ] **Step 3: Commit residue cleanup**

Run:

```powershell
git add .
git commit -m "refactor: remove AutoMapper residues"
```

---

### Task 7: Build Verification

**Files:**
- No planned source edits unless build errors expose missed references.

- [ ] **Step 1: Run full backend build**

Run:

```powershell
dotnet build NexusStack.sln
```

Expected: build succeeds.

- [ ] **Step 2: Fix missing namespace errors**

If build fails with `CS0246` for `IMapper`, add:

```csharp
using MapsterMapper;
```

If build fails with `CS0246` for `TypeAdapterConfig`, add:

```csharp
using Mapster;
```

If build fails because a `ServiceBase` constructor call has two arguments, add the third argument:

```csharp
TypeAdapterConfig mapperConfig
```

and pass it to base:

```csharp
: base(dbContext, mapper, mapperConfig)
```

- [ ] **Step 3: Run residue scan again**

Run the two residue scan commands from Task 6.

Expected: no AutoMapper source residues.

- [ ] **Step 4: Commit build fixes**

Run:

```powershell
git add .
git commit -m "fix: complete Mapster build migration"
```

---

### Task 8: Runtime Smoke Check

**Files:**
- No planned source edits unless startup exposes missing DI registration.

- [ ] **Step 1: Start WebAPI without long-running final claim**

Run:

```powershell
dotnet run --project Host/NexusStack.WebAPI/NexusStack.WebAPI.csproj
```

Expected startup signs:

```text
注册 Mapster
Now listening on:
```

Stop the process after startup succeeds.

- [ ] **Step 2: If DI fails for TypeAdapterConfig**

Confirm `AddAllMapster()` registers:

```csharp
services.AddSingleton(config);
services.AddScoped<IMapper, ServiceMapper>();
```

and every `ServiceBase` subclass constructor accepts and passes `TypeAdapterConfig`.

- [ ] **Step 3: Commit smoke-fix changes**

Only if fixes were needed, run:

```powershell
git add .
git commit -m "fix: resolve Mapster runtime registration"
```

---

### Task 9: Documentation Update

**Files:**
- Modify: `README.md`
- Modify: `Docs/superpowers/specs/2026-04-22-mapster-migration-design.md`

- [ ] **Step 1: Update README feature/dependency note if AutoMapper is mentioned**

Run:

```powershell
Select-String -Path README.md,Docs/**/*.md -Pattern 'AutoMapper'
```

If `README.md` mentions AutoMapper as a current dependency, replace that wording with `Mapster`.

- [ ] **Step 2: Mark design as implemented**

In `Docs/superpowers/specs/2026-04-22-mapster-migration-design.md`, change:

```markdown
> 状态：待评审
```

to:

```markdown
> 状态：已实施
```

- [ ] **Step 3: Commit docs update**

Run:

```powershell
git add README.md Docs/superpowers/specs/2026-04-22-mapster-migration-design.md
git commit -m "docs: document Mapster migration completion"
```

---

## Final Verification

- [ ] Run:

```powershell
dotnet build NexusStack.sln
```

Expected: build succeeds.

- [ ] Run:

```powershell
Get-ChildItem -Path . -Recurse -Include *.cs,*.csproj -File |
  Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\|\\.git\\' } |
  Select-String -Pattern 'AutoMapper|ProjectTo<|CreateMap\\(|Profile\\b|AddAutoMapper|Repository\\.AutoMapper|IAutoMapperRepository|AutoMapperRepository'
```

Expected: no source matches.

- [ ] Run:

```powershell
git status --short
```

Expected: clean working tree after final commit.
