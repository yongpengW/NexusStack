using AutoMapper;
using NexusStack.Core.Dtos;
using NexusStack.Core.Dtos.DownloadCenter;
using NexusStack.Core.Dtos.Files;
using NexusStack.Core.Dtos.GlobalSettings;
using NexusStack.Core.Dtos.Menus;
using NexusStack.Core.Dtos.Messages;
using NexusStack.Core.Dtos.NotifyEvent;
using NexusStack.Core.Dtos.Regions;
using NexusStack.Core.Dtos.Roles;
using NexusStack.Core.Dtos.ScheduleTasks;
using NexusStack.Core.Dtos.Users;
using NexusStack.Core.Entities.AsyncTasks;
using NexusStack.Core.Entities.Messages;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Entities.Users;
using NexusStack.Infrastructure.Utils;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;
using File = NexusStack.Core.Entities.SystemManagement.File;

namespace NexusStack.Core.MapProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Menu, MenuDto>()
                .ForMember(a => a.IsLeaf, a => a.MapFrom(c => c.Children.Count == 0));

            CreateMap<Menu, MenuTreeDto>()
                .ForMember(a => a.Children, a => a.Ignore());

            CreateMap<MenuDto, MenuTreeDto>();

            CreateMap<NotifyEvent, NotifyEventDto>()
             .ForMember(a => a.IsLeaf, a => a.MapFrom(c => c.Children.Count == 0));

            CreateMap<NotifyEvent, NotifyEventTreeDto>()
                .ForMember(a => a.Children, a => a.Ignore())
                .ForMember(a => a.NotifyRoles, a => a.MapFrom(c => c.NotifyRoles.toBigIntList()))
                .ForMember(a => a.MessageTypes, a => a.MapFrom(c => c.MessageTypes.toBigIntList()));

            CreateMap<SeedDataTask, SeedDataTaskDto>();

            CreateMap<ApiResource, ApiResourceDto>();

            CreateMap<ScheduleTask, ScheduleTaskDto>();

            CreateMap<ApiResourceDto, MenuResourceDto>();

            CreateMap<Role, RoleDto>();

            CreateMap<File, FileDto>();

            CreateMap<User, UserDto>()
                //.ForMember(a => a.Roles, a => a.MapFrom(c => c.UserRoles))
                .ForMember(a => a.HasPassword, a => a.MapFrom(c => !string.IsNullOrWhiteSpace(c.Password)));

            CreateMap<UserRole, UserRoleDto>()
                //.ForMember(a => a.RegionName, a => a.MapFrom(c => c.Region.Name))
                //.ForMember(a => a.RoleName, a => a.MapFrom(c => c.Role.Name))
                .ForMember(a => a.Platforms, a => a.MapFrom(c => c.Role.Platforms));

            CreateMap<Region, RegionDto>();

            CreateMap<Region, RegionTreeDto>()
                .ForMember(a => a.Children, a => a.Ignore());

            CreateMap<RegionDto, RegionTreeDto>();

            CreateMap<ScheduleTask, ScheduleTaskExecuteDto>();

            CreateMap<ScheduleTaskExecuteDto, ScheduleTask>();

            CreateMap<UserToken, UserTokenCacheDto>();
            //.ForMember(a => a.Roles, a => a.MapFrom(c => c.User.UserRoles.Select(r => r.Role.Code).ToList()));
            //.ForMember(a => a.PopulationId, a => a.MapFrom(c => c.User.PopulationId));

            CreateMap<UserToken, UserTokenDto>();

            CreateMap<UserTokenCacheDto, UserTokenDto>();

            CreateMap<Permission, RolePermissionDto>();

            CreateMap<User, CurrentUserDto>()
                //.ForMember(a => a.Roles, a => a.MapFrom(c => c.UserRoles))
                .ForMember(a => a.HasPassword, a => a.MapFrom(c => !string.IsNullOrWhiteSpace(c.Password)));

            CreateMap<AsyncTask, AsyncTaskDto>();

            CreateMap<Options, OptionsDto>();

            CreateMap<File, ExportFileDto>();

            CreateMap<GlobalSettings, GlobalSettingDto>();

            CreateMap<SMSMessage, SMSMessageDto>();

            CreateMap<DownloadItem, DownloadItemDto>();
        }
    }
}
