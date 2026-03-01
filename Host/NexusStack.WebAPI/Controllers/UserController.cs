using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        IUserContextCacheService userContextCacheService
    ) : BaseController
    {
        /// <summary>
        /// 获取用户分页列表
        /// </summary>
        [HttpGet("list"), NoLogging]
        public async Task<IPagedList<UserDto>> GetListAsync([FromQuery] UserQueryDto model)
        {
            var filter = PredicateBuilder.New<User>(true);

            if (!string.IsNullOrEmpty(model.UserName))
                filter.And(a => a.UserName.Contains(model.UserName));
            if (!string.IsNullOrEmpty(model.Mobile))
                filter.And(a => a.Mobile!.Contains(model.Mobile));
            if (!string.IsNullOrEmpty(model.Email))
                filter.And(a => a.Email.Contains(model.Email));
            if (model.IsEnable.HasValue)
                filter.And(a => a.IsEnable == model.IsEnable.Value);

            var userQuery = userService.GetExpandable().Where(filter);

            // 按角色筛选时使用子查询，避免 INNER JOIN 导致无角色用户被排除或产生重复行
            if (model.RoleId.HasValue && model.RoleId != 0)
            {
                var userIdsWithRole = userRoleService.GetQueryable()
                    .Where(ur => ur.RoleId == model.RoleId.Value)
                    .Select(ur => ur.UserId);
                userQuery = userQuery.Where(u => userIdsWithRole.Contains(u.Id));
            }

            var query = userQuery
                .OrderByDescending(a => a.Id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Mobile = u.Mobile,
                    Email = u.Email,
                    NickName = u.NickName,
                    RealName = u.RealName,
                    Gender = u.Gender,
                    IsEnable = u.IsEnable,
                    LastLoginTime = u.LastLoginTime,
                    Avatar = u.Avatar,
                });

            var list = query.ToPagedList(model.Page, model.Limit);

            if (list.Count > 0)
            {
                var userIds = list.Select(a => a.Id).ToList();

                // 批量加载用户-角色关联（含 Role 导航属性）
                var allUserRoles = await userRoleService.GetQueryable()
                    .Where(a => userIds.Contains(a.UserId))
                    .Include(a => a.Role)
                    .ToListAsync();

                // 批量加载用户-组织单元关联
                var allDepartments = await userService.GetDbContext.Set<UserDepartment>()
                    .Where(a => userIds.Contains(a.UserId))
                    .ToListAsync();

                foreach (var item in list)
                {
                    item.UserRoles = allUserRoles
                        .Where(ur => ur.UserId == item.Id)
                        .Select(ur => new UserRoleDto
                        {
                            Id = ur.Id,
                            RoleId = ur.RoleId,
                            RoleName = ur.Role?.Name ?? string.Empty,
                            Platforms = ur.Role?.Platforms ?? default,
                        }).ToList();

                    item.Departments = allDepartments
                        .Where(d => d.UserId == item.Id)
                        .Select(d => new UserDepartmentDto
                        {
                            UserId = d.UserId,
                            DepartmentId = d.DepartmentId,
                        }).ToList();
                }
            }

            return list;
        }

        /// <summary>
        /// 获取用户详情
        /// </summary>
        [HttpGet("{id}"), NoLogging]
        public async Task<UserDto> GetByIdAsync(long id)
        {
            var user = await userService.GetByIdAsync<UserDto>(id);
            if (user is null)
                throw new BusinessException("用户不存在");

            await PopulateUserRolesAndDepartmentsAsync(user, id);
            return user;
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        [HttpGet("me"), NoLogging]
        public async Task<CurrentUserDto> GetCurrentAsync()
        {
            var user = await userService.GetAsync<CurrentUserDto>(x => x.Id == CurrentUser.UserId);
            if (user is null)
                throw new BusinessException("用户不存在");

            var userRoles = await userRoleService.GetQueryable()
                .Where(a => a.UserId == CurrentUser.UserId)
                .Include(a => a.Role)
                .ToListAsync();

            user.UserRoles = userRoles.Select(ur => new UserRoleDto
            {
                Id = ur.Id,
                RoleId = ur.RoleId,
                RoleName = ur.Role?.Name ?? string.Empty,
                Platforms = ur.Role?.Platforms ?? default,
            }).ToList();

            user.Departments = await userService.GetDbContext.Set<UserDepartment>()
                .Where(d => d.UserId == CurrentUser.UserId)
                .Select(d => new UserDepartmentDto { UserId = d.UserId, DepartmentId = d.DepartmentId })
                .ToListAsync();

            return user;
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        [HttpPost]
        public async Task<long> PostAsync(CreateUserDto model)
        {
            if (model.UserName.IsNullOrEmpty())
                throw new BusinessException("账号不能为空");
            if (model.Mobile.IsNullOrEmpty())
                throw new BusinessException("手机号码不能为空");
            if (model.UserRoles is not { Count: > 0 })
                throw new BusinessException("请为用户选择角色");

            var entity = this.Mapper.Map<User>(model);
            // 默认密码为手机号后 6 位，由 UserService.InsertAsync 负责加盐加密
            entity.Password = model.Mobile![^6..];

            var strategy = userService.GetDbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var trans = await userService.BeginTransactionAsync();
                try
                {
                    // 必须先 Insert User，entity.Id 由数据库生成后方可用于关联
                    await userService.InsertAsync(entity);

                    var roles = model.UserRoles.Select(item => new UserRole
                    {
                        RoleId = item.RoleId,
                        UserId = entity.Id,
                    }).ToList();
                    await userRoleService.InsertAsync(roles);

                    if (model.DepartmentIds is { Length: > 0 })
                    {
                        var departments = model.DepartmentIds.Select(deptId => new UserDepartment
                        {
                            UserId = entity.Id,
                            DepartmentId = deptId,
                        }).ToList();
                        await userService.GetDbContext.Set<UserDepartment>().AddRangeAsync(departments);
                        await userService.GetDbContext.SaveChangesAsync();
                    }

                    await trans.CommitAsync();
                }
                catch
                {
                    await userService.RollbackAsync(trans);
                    throw;
                }
            });

            return entity.Id;
        }

        /// <summary>
        /// 修改用户信息
        /// </summary>
        [HttpPut("{id}")]
        public async Task<StatusCodeResult> PutAsync(long id, CreateUserDto model)
        {
            var entity = await userService.GetAsync(item => item.Id == id);
            if (entity is null)
                throw new BusinessException("你要修改的用户不存在");

            entity = this.Mapper.Map(model, entity);

            var strategy = userService.GetDbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var trans = await userService.BeginTransactionAsync();
                try
                {
                    // 重建用户-角色关联
                    await userRoleService.BatchDeleteAsync(a => a.UserId == id);
                    var roles = (model.UserRoles ?? []).Select(item => new UserRole
                    {
                        RoleId = item.RoleId,
                        UserId = id,
                    }).ToList();
                    await userRoleService.InsertAsync(roles);

                    // 重建用户-组织单元关联
                    var oldDepartments = await userService.GetDbContext.Set<UserDepartment>()
                        .Where(d => d.UserId == id)
                        .ToListAsync();
                    if (oldDepartments.Count > 0)
                    {
                        userService.GetDbContext.Set<UserDepartment>().RemoveRange(oldDepartments);
                        await userService.GetDbContext.SaveChangesAsync();
                    }
                    if (model.DepartmentIds is { Length: > 0 })
                    {
                        var departments = model.DepartmentIds.Select(deptId => new UserDepartment
                        {
                            UserId = id,
                            DepartmentId = deptId,
                        }).ToList();
                        await userService.GetDbContext.Set<UserDepartment>().AddRangeAsync(departments);
                        await userService.GetDbContext.SaveChangesAsync();
                    }

                    entity.UserRoles = null;
                    await userService.UpdateAsync(entity);

                    await trans.CommitAsync();
                }
                catch
                {
                    await userService.RollbackAsync(trans);
                    throw;
                }
            });

            // 角色/组织变更后，使该用户所有平台的权限缓存失效
            await userContextCacheService.InvalidateAsync(id);

            return Ok();
        }

        /// <summary>
        /// 启用用户
        /// </summary>
        [HttpPut("enable/{id}")]
        public async Task<StatusCodeResult> EnableAsync(long id)
        {
            var entity = await userService.GetByIdAsync(id);
            if (entity is null)
                throw new BusinessException("你要启用的用户不存在");

            entity.IsEnable = true;
            await userService.UpdateAsync(entity);
            await userContextCacheService.InvalidateAsync(id);

            return Ok();
        }

        /// <summary>
        /// 禁用用户
        /// </summary>
        [HttpPut("disable/{id}")]
        public async Task<StatusCodeResult> DisableAsync(long id)
        {
            if (CurrentUser.UserId == id)
                throw new BusinessException("不能禁用当前登录的用户");

            var entity = await userService.GetByIdAsync(id);
            if (entity is null)
                throw new BusinessException("你要禁用的用户不存在");

            entity.IsEnable = false;
            await userService.UpdateAsync(entity);

            // 用户被禁用后，使其所有平台的权限缓存全部失效
            await userContextCacheService.InvalidateAsync(id);

            return Ok();
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<StatusCodeResult> DeleteAsync(long id)
        {
            var entity = await userService.GetAsync(a => a.Id == id);
            if (entity is null)
                throw new BusinessException("你要删除的用户不存在");

            if (CurrentUser.UserId == entity.Id)
                throw new BusinessException("不能删除当前登录的用户");

            var strategy = userService.GetDbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var trans = await userService.BeginTransactionAsync();
                try
                {
                    await userRoleService.BatchDeleteAsync(x => x.UserId == id);

                    var departments = await userService.GetDbContext.Set<UserDepartment>()
                        .Where(d => d.UserId == id)
                        .ToListAsync();
                    if (departments.Count > 0)
                    {
                        userService.GetDbContext.Set<UserDepartment>().RemoveRange(departments);
                        await userService.GetDbContext.SaveChangesAsync();
                    }

                    await userService.DeleteAsync(entity);
                    await trans.CommitAsync();
                }
                catch
                {
                    await userService.RollbackAsync(trans);
                    throw;
                }
            });

            // 清除权限缓存
            await userContextCacheService.InvalidateAsync(id);

            return Ok();
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        [HttpPut("reset/{id}")]
        public async Task<StatusCodeResult> ResetPasswordAsync(long id)
        {
            await userService.ResetPasswordAsync(id);
            return Ok();
        }

        // ── 私有辅助方法 ─────────────────────────────────────────────────────────

        private async Task PopulateUserRolesAndDepartmentsAsync(UserDto user, long userId)
        {
            var userRoles = await userRoleService.GetQueryable()
                .Where(a => a.UserId == userId)
                .Include(a => a.Role)
                .ToListAsync();

            user.UserRoles = userRoles.Select(ur => new UserRoleDto
            {
                Id = ur.Id,
                RoleId = ur.RoleId,
                RoleName = ur.Role?.Name ?? string.Empty,
                Platforms = ur.Role?.Platforms ?? default,
            }).ToList();

            user.Departments = await userService.GetDbContext.Set<UserDepartment>()
                .Where(d => d.UserId == userId)
                .Select(d => new UserDepartmentDto { UserId = d.UserId, DepartmentId = d.DepartmentId })
                .ToListAsync();
        }
    }
}
