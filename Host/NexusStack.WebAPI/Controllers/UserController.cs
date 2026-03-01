using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ardalis.Specification;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos.Users;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.Utils;
using X.PagedList;
using X.PagedList.Extensions;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class UserController(
        IUserService userService,
        IUserRoleService userRoleService,
        IRegionService regionService,
        IConfiguration configuration
    ) : BaseController
    {
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("list"), NoLogging]
        public async Task<IPagedList<UserDto>> GetListAsync([FromQuery] UserQueryDto model)
        {
            var filter = PredicateBuilder.New<User>(true);
            var userRoleFilter = PredicateBuilder.New<UserRole>(true);

            //if (!string.IsNullOrWhiteSpace(model.Keyword))
            //{
            //    filter.Or(a => a.UserName.Contains(model.Keyword));
            //    filter.Or(a => a.Mobile.Contains(model.Keyword));
            //    filter.Or(a => a.NickName.Contains(model.Keyword));
            //    filter.Or(a => a.RealName.Contains(model.Keyword));
            //}

            if (!string.IsNullOrEmpty(model.UserName))
                filter.And(a => a.UserName.Contains(model.UserName));
            if (!string.IsNullOrEmpty(model.Mobile))
                filter.And(a => a.Mobile.Contains(model.Mobile));
            if (!string.IsNullOrEmpty(model.Email))
                filter.And(a => a.Email.Contains(model.Email));

            if (model.IsEnable.HasValue)
            {
                filter.And(a => a.IsEnable == model.IsEnable.Value);
            }

            if (model.RoleId.HasValue && model.RoleId != 0)
            {
                userRoleFilter.And(item => item.RoleId == model.RoleId.Value);
            }

            var query = (from u in userService.GetExpandable().Where(filter)
                         join ur in userRoleService.GetExpandable().Where(userRoleFilter) on u.Id equals ur.UserId
                         select new UserDto
                         {
                             Id = u.Id,
                             UserName = u.UserName,
                             Mobile = u.Mobile,
                             Email = u.Email,
                             NickName = u.NickName,
                             RealName = u.RealName,
                             Gender = u.Gender,
                             IsEnable = u.IsEnable,
                             Remark = u.Remark,
                         })
                        .Distinct()
                        .OrderByDescending(a => a.Id);

            var list = query.ToPagedList(model.Page, model.Limit);

            var userIds = list.Select(a => a.Id).ToList();
            var allRoles = await userRoleService.GetQueryable()
                .Where(a => userIds.Contains(a.UserId)).Include(a => a.Role).ToListAsync();
            //var shops = await shopService.GetListAsync();
            var regions = await regionService.GetListAsync();



            foreach (var item in list)
            {
                var userRoles = allRoles.Where(x => x.UserId == item.Id);
                //var departmentIds = item.DepartmentIdsValue?.Split('.').Select(a => a).ToArray();

                //if (departmentIds != null && !string.IsNullOrWhiteSpace(item.DepartmentIdsValue))
                //{
                //    var departments = new List<UserDepartmentDto>();

                //    foreach (var depart in departmentIds)
                //    {
                //        var region = regions.FirstOrDefault(a => a.Id == long.Parse(depart));
                //        if (region != null)
                //        {
                //            departments.Add(new UserDepartmentDto
                //            {
                //                DepartmentId = region.Id,
                //                DepartmentName = region.Name
                //            });
                //        }

                //        var shop = shops.FirstOrDefault(a => a.Id == long.Parse(depart));
                //        if (shop != null)
                //        {
                //            departments.Add(new UserDepartmentDto
                //            {
                //                DepartmentId = shop.Id,
                //                DepartmentName = shop.ShopName
                //            });
                //        }
                //    }
                //    item.DepartmentIds = departmentIds;
                //    item.Departments = departments;
                //}
            }
            return list;
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<long> PostAsync(CreateUserDto model)
        {
            if (model.UserName.IsNullOrEmpty())
                throw new BusinessException("账号不能为空");
            else if (model.Mobile.IsNullOrEmpty())
                throw new BusinessException("手机号码不能为空");
            else if (model.UserRoles.IsNull())
                throw new BusinessException("请为用户选择角色");
            else if ((await userService.GetCountAsync(x => x.UserName.ToLower() == model.UserName.ToLower())) > 0)
                throw new BusinessException("账号已存在");

            var entity = this.Mapper.Map<User>(model);

            var roles = new List<UserRole>();

            model.UserRoles.ForEach((item =>
            {
                roles.Add(new UserRole
                {
                    RoleId = item.RoleId,
                    UserId = entity.Id,
                    //RegionId = item.RegionId
                });
            }));

            await userRoleService.InsertAsync(roles);

            //entity.UserRoles = model.Roles.Select(a => new UserRole
            //{
            //    RoleId = a.RoleId,
            //    UserId = entity.Id,
            //    RegionId = a.RegionId
            //}).ToList();

            //if (model.Departments.IsNotNull())// 插入新部门
            //    await userDepartmentService.InsertAsync(model.Departments.Select(a => new UserDepartment
            //    {
            //        UserId = entity.Id,
            //        RegionId = a.RegionId,
            //        DepartmentId = a.DepartmentId
            //    }).ToList());

            // 设置默认密码为手机号码后 6 位
            entity.Password = model.Mobile[^6..];
            await userService.InsertAsync(entity);
            return entity.Id;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("{id}")]
        public async Task<StatusCodeResult> DeleteAsync(long id)
        {
            var entity = await userService.GetAsync(a => a.Id == id);
            if (entity is null)
            {
                throw new Exception("你要删除的用户不存在");
            }

            if (CurrentUser.UserId == entity.Id)
            {
                throw new Exception("你不能删除你当前登录的用户");
            }

            //删除用户角色
            await userRoleService.BatchDeleteAsync(x => x.UserId == id);

            await userService.DeleteAsync(entity);

            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, configuration["SSO:ConnectDeleteUser"]);
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", entity.Email),
             });

            request.Content = requestContent;
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                }
            }

            return Ok();
        }

        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<StatusCodeResult> PutAsync(long id, CreateUserDto model)
        {
            var entity = await userService.GetAsync(item => item.Id == id);
            if (entity is null)
            {
                throw new BusinessException("你要修改的用户不存在");
            }

            entity = this.Mapper.Map(model, entity);

            var strategy = userService.GetDbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var trans = await userService.BeginTransactionAsync();

                try
                {
                    await userRoleService.BatchDeleteAsync(a => a.UserId == id);

                    foreach (var item in model.UserRoles)
                    {
                        var userRole = new UserRole
                        {
                            RoleId = item.RoleId,
                            UserId = entity.Id,
                            //RegionId = item.RegionId
                        };
                        await userRoleService.InsertAsync(userRole);
                    }

                    entity.UserRoles = null;

                    //entity.DepartmentIds = string.Join(".", model.DepartmentIds.Select(a => a));

                    await userService.UpdateAsync(entity);

                    await trans.CommitAsync();
                }
                catch (Exception ex)
                {
                    await userService.RollbackAsync(trans);
                    throw new Exception(ex.Message);
                }
            });
            return Ok();
        }

        /// <summary>
        /// 启用用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("enable/{id}")]
        public async Task<StatusCodeResult> EnableAsync(long id)
        {
            var entity = await userService.GetByIdAsync(id);
            if (entity == null)
            {
                throw new BusinessException("你要启用的数据不存在");
            }

            entity.IsEnable = true;

            await userService.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// 禁用用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("disable/{id}")]
        public async Task<StatusCodeResult> DisableAsync(long id)
        {
            var entity = await userService.GetByIdAsync(id);
            if (entity == null)
            {
                throw new BusinessException("你要禁用的数据不存在");
            }

            entity.IsEnable = false;

            await userService.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("reset/{id}")]
        public async Task<StatusCodeResult> ResetPasswordAsync(long id)
        {
            await userService.ResetPasswordAsync(id);
            return Ok();
        }
    }
}
