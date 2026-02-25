using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NexusStack.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core
{
    /// <summary>
    /// 项目中所有控制器的基类
    /// </summary>
    [ApiController, Authorize, Route("api/[controller]")]
    //[Authorize(AuthenticationSchemes = "Authorization-Token")]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// 可直接使用AutoMapper
        /// </summary>
        public IMapper Mapper
        {
            get
            {
                return HttpContext.RequestServices.GetRequiredService<IMapper>();
            }
        }

        /// <summary>
        /// 当前用户
        /// </summary>
        public ICurrentUser CurrentUser
        {
            get
            {
                return HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
            }
        }
    }
}
