using Microsoft.AspNetCore.Mvc;
using NexusStack.Core;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// 基础服务控制器基类，继承自公共服务的控制器基类
    /// </summary>
    [Route("api/[controller]")]
    public class BaseController : ApiControllerBase
    {

    }
}
