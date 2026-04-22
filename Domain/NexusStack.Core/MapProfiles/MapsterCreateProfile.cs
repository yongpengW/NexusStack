using Mapster;
using NexusStack.Core.Dtos.GlobalSettings;
using NexusStack.Core.Dtos.Menus;
using NexusStack.Core.Dtos.Regions;
using NexusStack.Core.Dtos.Roles;
using NexusStack.Core.Dtos.Users;
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
