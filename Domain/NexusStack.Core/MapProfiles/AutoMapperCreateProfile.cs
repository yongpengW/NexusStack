using AutoMapper;
using NexusStack.Core.Dtos.GlobalSettings;
using NexusStack.Core.Dtos.Menus;
using NexusStack.Core.Dtos.Messages;
using NexusStack.Core.Dtos.Regions;
using NexusStack.Core.Dtos.Roles;
using NexusStack.Core.Dtos.Users;
using NexusStack.Core.Entities.Messages;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Entities.Users;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.MapProfiles
{
    /// <summary>
    /// 新增和修改数据的映射文件
    /// </summary>
    public class AutoMapperCreateProfile : Profile
    {
        public AutoMapperCreateProfile()
        {
            CreateMap<CreateMenuDto, Menu>();

            CreateMap<CreateRoleDto, Role>();

            CreateMap<CreateRegionDto, Region>();

            CreateMap<CreateUserDto, User>()
                .ForMember(a => a.DepartmentIds, a => a.MapFrom(c => string.Join('.', c.DepartmentIds.Select(x => x))));

            CreateMap<CreateUserRoleDto, UserRole>();

            CreateMap<CreateGlobalSettingDto, GlobalSettings>();

            CreateMap<CreateSMSMessageDto, SMSMessage>();

            CreateMap<CreateInternalMessageDto, InternalMessage>();
        }
    }
}
