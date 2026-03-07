using AutoMapper;
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
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region User & Role

            CreateMap<User, UserDto>()
               .ForMember(a => a.Roles, a => a.Ignore())
               .ForMember(a => a.UserRoles, a => a.Ignore())
               .ForMember(a => a.Departments, a => a.Ignore())
               .ForMember(a => a.HasPassword, a => a.MapFrom(c => !string.IsNullOrWhiteSpace(c.Password)));

            CreateMap<UserDepartment, UserDepartmentDto>();

            CreateMap<Role, RoleDto>();

            CreateMap<UserRole, UserRoleDto>()
                .ForMember(a => a.RoleName, a => a.MapFrom(c => c.Role != null ? c.Role.Name : string.Empty))
                .ForMember(a => a.Platforms, a => a.MapFrom(c => c.Role != null ? c.Role.Platforms : PlatformType.All));

            CreateMap<UserToken, UserTokenDto>()
                .ForMember(a => a.UserName, a => a.MapFrom(c => c.User != null ? c.User.UserName : string.Empty));

            CreateMap<UserTokenCacheDto, UserTokenDto>();

            CreateMap<Permission, RolePermissionDto>();

            CreateMap<User, CurrentUserDto>()
                //.ForMember(a => a.Roles, a => a.MapFrom(c => c.UserRoles))
                .ForMember(a => a.HasPassword, a => a.MapFrom(c => !string.IsNullOrWhiteSpace(c.Password)));

            CreateMap<UserToken, UserTokenCacheDto>()
                .ForMember(a => a.UserName, a => a.MapFrom(c => c.User != null ? c.User.UserName : string.Empty));

            CreateMap<UserToken, UserTokenLogDto>()
                .ForMember(a => a.loginUser, a => a.MapFrom(c => c.User != null ? (c.User.RealName ?? c.User.UserName) : string.Empty))
                .ForMember(a => a.loginAt, a => a.MapFrom(c => c.CreatedAt));

            #endregion

            CreateMap<Menu, MenuDto>();

            CreateMap<Menu, MenuTreeDto>()
                .ForMember(a => a.Children, a => a.Ignore());

            CreateMap<MenuDto, MenuTreeDto>();

            CreateMap<SeedDataTask, SeedDataTaskDto>();

            CreateMap<ApiResource, ApiResourceDto>();

            CreateMap<ScheduleTask, ScheduleTaskDto>();

            CreateMap<ApiResourceDto, MenuResourceDto>();

            CreateMap<File, FileDto>();

            CreateMap<Region, RegionDto>();

            CreateMap<Region, RegionTreeDto>()
                .ForMember(a => a.Children, a => a.Ignore());

            CreateMap<RegionDto, RegionTreeDto>();

            CreateMap<ScheduleTask, ScheduleTaskExecuteDto>();

            CreateMap<ScheduleTaskExecuteDto, ScheduleTask>();

            

            

            CreateMap<AsyncTask, AsyncTaskDto>();

            CreateMap<Options, OptionsDto>();

            CreateMap<File, ExportFileDto>();

            CreateMap<GlobalSettings, GlobalSettingDto>();

            CreateMap<DownloadItem, DownloadItemDto>();
        }
    }
}
