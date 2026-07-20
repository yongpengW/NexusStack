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
            #region User & Role

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
                //.Map(a => a.Roles, c => c.UserRoles)
                .Map(a => a.HasPassword, c => !string.IsNullOrWhiteSpace(c.Password));

            config.NewConfig<UserToken, UserTokenCacheDto>()
                .Map(a => a.UserName, c => c.User != null ? c.User.UserName : string.Empty);

            config.NewConfig<UserToken, UserTokenLogDto>()
                .Map(a => a.loginUser, c => c.User != null ? (c.User.RealName ?? c.User.UserName) : string.Empty)
                .Map(a => a.loginAt, c => c.CreatedAt);

            #endregion

            config.NewConfig<Menu, MenuDto>();

            config.NewConfig<Menu, MenuTreeDto>()
                .Ignore(a => a.Children);

            config.NewConfig<MenuDto, MenuTreeDto>();

            config.NewConfig<SeedDataTask, SeedDataTaskDto>();

            config.NewConfig<ApiResource, ApiResourceDto>();

            config.NewConfig<ScheduleTask, ScheduleTaskDto>();

            config.NewConfig<ApiResourceDto, MenuResourceDto>();

            config.NewConfig<File, FileDto>();

            config.NewConfig<Region, RegionDto>();

            config.NewConfig<Region, RegionTreeDto>()
                .Ignore(a => a.Children);

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
