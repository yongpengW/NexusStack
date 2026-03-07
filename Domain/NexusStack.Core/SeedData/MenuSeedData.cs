using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.DbContexts;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;

namespace NexusStack.Core.SeedData
{
    /// <summary>
    /// 初始化菜单数据
    /// </summary>
    /// <param name="scopeFactory"></param>
    public class MenuSeedData(IServiceScopeFactory scopeFactory) : ISeedData, ITransientDependency
    {
        public int Order => 4;

        public string ConfigPath { get; set; } = string.Empty;

        public async Task ApplyAsync(SeedDataTask model)
        {
            using var scope = scopeFactory.CreateScope();
            var menuService = scope.ServiceProvider.GetRequiredService<IMenuService>();

            Menu Create(
                long id,
                string name,
                string code,
                long parentId,
                MenuType type,
                string? icon,
                string? activeIcon,
                string? url,
                int order,
                bool isVisible,
                string idSequences,
                string? remark = null)
            {
                return new Menu
                {
                    Id = id,
                    Name = name,
                    Code = code,
                    ParentId = parentId,
                    Type = type,
                    PlatformType = PlatformType.Pc,
                    Icon = icon,
                    IconType = MenuIconType.Icon,
                    ActiveIcon = activeIcon,
                    ActiveIconType = MenuIconType.Icon,
                    Url = url,
                    Order = order,
                    IsVisible = isVisible,
                    IsExternalLink = false,
                    IdSequences = idSequences,
                    SystemId = 0,
                    Remark = remark
                };
            }

            var data = new List<Menu>
            {
                Create(2029815869164621824, "首页", "home", 0, MenuType.Directory, "HomeOutlined", "HomeOutlined", "/home", 1, true, ".2029815869164621824."),
                Create(2029816008180633600, "仪表盘", "dashboard", 0, MenuType.Directory, "DashboardOutlined", "DashboardOutlined", "/dashboard", 2, true, ".2029816008180633600."),
                Create(2029816298611019776, "分析页", "dashboard.analysis", 2029816008180633600, MenuType.Menu, "BarChartOutlined", "BarChartOutlined", "/dashboard/analysis", 1, true, ".2029816008180633600.2029816298611019776."),
                Create(2029819467558686720, "监控页", "dashboard.monitor", 2029816008180633600, MenuType.Menu, "MonitorOutlined", "MonitorOutlined", "/dashboard/monitor", 2, true, ".2029816008180633600.2029819467558686720."),
                Create(2029819625742667776, "工作台", "dashboard.workplace", 2029816008180633600, MenuType.Menu, "DesktopOutlined", "DesktopOutlined", "/dashboard/workplace", 3, true, ".2029816008180633600.2029819625742667776."),
                Create(2029551849073414144, "账户", "account", 0, MenuType.Directory, "UserOutlined", "UserOutlined", "/account", 3, true, ".2029551849073414144."),
                Create(2029552075138011136, "个人中心", "account.center", 2029551849073414144, MenuType.Menu, "UserOutlined", "UserOutlined", "/account/center", 1, true, ".2029551849073414144.2029552075138011136."),
                Create(2029552287063609344, "账户设置", "account.settings", 2029551849073414144, MenuType.Menu, "SettingOutlined", "SettingOutlined", "/account/settings", 2, true, ".2029551849073414144.2029552287063609344."),
                Create(2029819896669540352, "表单页", "form", 0, MenuType.Directory, "FormOutlined", "FormOutlined", "/form", 3, true, ".2029819896669540352."),
                Create(2029820185409622016, "基础表单", "form.basic-form", 2029819896669540352, MenuType.Menu, "FileTextOutlined", "FileTextOutlined", "/form/basic-form", 1, true, ".2029819896669540352.2029820185409622016."),
                Create(2029820377101897728, "分布表单", "form.step-form", 2029819896669540352, MenuType.Menu, "StepForwardOutlined", "StepForwardOutlined", "/form/step-form", 2, true, ".2029819896669540352.2029820377101897728."),
                Create(2029820577769984000, "高级表单", "form.advanced-form", 2029819896669540352, MenuType.Menu, "ControlOutlined", "ControlOutlined", "/form/advanced-form", 3, true, ".2029819896669540352.2029820577769984000."),
                Create(2029820811631792128, "列表页", "list", 0, MenuType.Directory, "UnorderedListOutlined", "UnorderedListOutlined", "/list", 4, true, ".2029820811631792128."),
                Create(2029821324448370688, "查询表格", "list.table-list", 2029820811631792128, MenuType.Menu, "TableOutlined", "TableOutlined", "/list/table-list", 1, true, ".2029820811631792128.2029821324448370688."),
                Create(2029821509379428352, "标准列表", "list.basic-list", 2029820811631792128, MenuType.Menu, "OrderedListOutlined", "OrderedListOutlined", "/list/basic-list", 2, true, ".2029820811631792128.2029821509379428352."),
                Create(2029821677378080768, "卡片列表", "list.card-list", 2029820811631792128, MenuType.Menu, "AppstoreOutlined", "AppstoreOutlined", "/list/card-list", 3, true, ".2029820811631792128.2029821677378080768."),
                Create(2029821884010467328, "搜索列表", "list.search-list", 2029820811631792128, MenuType.Menu, "UnorderedListOutlined", "UnorderedListOutlined", "/list/search", 4, true, ".2029820811631792128.2029821884010467328."),
                Create(2029822399293296640, "详情页", "profile", 0, MenuType.Directory, "ProfileOutlined", "ProfileOutlined", "/profile", 5, true, ".2029822399293296640."),
                Create(2029822614159101952, "基础详情页", "profile.basic", 2029822399293296640, MenuType.Menu, "FileTextOutlined", "FileTextOutlined", "/profile/basic", 1, true, ".2029822399293296640.2029822614159101952."),
                Create(2029822744971055104, "高级详情页", "profile.advanced", 2029822399293296640, MenuType.Menu, "ProfileOutlined", "ProfileOutlined", "/profile/advanced", 2, true, ".2029822399293296640.2029822744971055104."),
                Create(2029823006133587968, "结果页", "result", 0, MenuType.Directory, "CheckCircleOutlined", "CheckCircleOutlined", "/result", 6, true, ".2029823006133587968."),
                Create(2029823321213898752, "成功页", "result.success", 2029823006133587968, MenuType.Menu, "CheckCircleOutlined", "CheckCircleOutlined", "/result/success", 1, true, ".2029823006133587968.2029823321213898752."),
                Create(2029823454550822912, "失败页", "result.fail", 2029823006133587968, MenuType.Menu, "WarningOutlined", "WarningOutlined", "/result/fail", 2, true, ".2029823006133587968.2029823454550822912."),
                Create(2029823642606637056, "异常页", "exception", 0, MenuType.Directory, "WarningOutlined", "WarningOutlined", "/exception", 7, true, ".2029823642606637056."),
                Create(2029823883233857536, "403", "exception.403", 2029823642606637056, MenuType.Menu, "WarningOutlined", "WarningOutlined", "/exception/403", 1, true, ".2029823642606637056.2029823883233857536."),
                Create(2029824307382849536, "404", "exception.404", 2029823642606637056, MenuType.Menu, "WarningOutlined", "WarningOutlined", "/exception/404", 2, true, ".2029823642606637056.2029824307382849536."),
                Create(2029824448219189248, "500", "exception.500", 2029823642606637056, MenuType.Menu, "WarningOutlined", "WarningOutlined", "/exception/500", 3, true, ".2029823642606637056.2029824448219189248."),
                Create(2029098077972992000, "系统管理", "system", 0, MenuType.Directory, "SettingOutlined", "SettingOutlined", "/system", 10, true, ".2029098077972992000."),
                Create(2029132655156662272, "用户管理", "system.user", 2029098077972992000, MenuType.Menu, "UserOutlined", "UserOutlined", "/system/user", 1, true, ".2029098077972992000.2029132655156662272."),
                Create(2029132201593016320, "角色管理", "system.role", 2029098077972992000, MenuType.Menu, "TeamOutlined", "TeamOutlined", "/system/role", 2, true, ".2029098077972992000.2029132201593016320."),
                Create(2029132372791922688, "菜单管理", "system.menu", 2029098077972992000, MenuType.Menu, "AppstoreAddOutlined", "AppstoreAddOutlined", "/system/menu", 3, true, ".2029098077972992000.2029132372791922688."),
                Create(2030110927914930176, "权限管理", "system.permission", 2029098077972992000, MenuType.Menu, "SafetyCertificateOutlined", "SafetyCertificateOutlined", "/system/permission", 4, true, ".2029098077972992000.2030110927914930176."),
                Create(2029128378505891840, "区域管理", "system.region", 2029098077972992000, MenuType.Menu, "ApartmentOutlined", "ApartmentOutlined", "/system/region", 5, true, ".2029098077972992000.2029128378505891840."),
                Create(2029131958226915328, "组织架构", "system.org", 2029098077972992000, MenuType.Menu, "ClusterOutlined", "ClusterOutlined", "/system/org", 6, true, ".2029098077972992000.2029131958226915328."),
                Create(2029186920067764224, "新增用户", "user.create", 2029132655156662272, MenuType.Operation, null, null, null, 1, false, ".2029098077972992000.2029132655156662272.2029186920067764224.")
            };

            foreach (var item in data)
            {
                var exists = await menuService.GetAsync(a => a.Id == item.Id);
                if (exists is null)
                {
                    await menuService.InsertAsync(item);
                    continue;
                }

                exists.Name = item.Name;
                exists.Code = item.Code;
                exists.ParentId = item.ParentId;
                exists.Type = item.Type;
                exists.PlatformType = item.PlatformType;
                exists.Icon = item.Icon;
                exists.IconType = item.IconType;
                exists.ActiveIcon = item.ActiveIcon;
                exists.ActiveIconType = item.ActiveIconType;
                exists.Url = item.Url;
                exists.Order = item.Order;
                exists.IsVisible = item.IsVisible;
                exists.IsExternalLink = item.IsExternalLink;
                exists.IdSequences = item.IdSequences;
                exists.SystemId = item.SystemId;
                exists.IsDeleted = false;
                exists.Remark = item.Remark;

                await menuService.UpdateAsync(exists);
            }
        }
    }
}
