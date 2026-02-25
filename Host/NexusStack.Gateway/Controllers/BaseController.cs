using Microsoft.AspNetCore.Mvc;
using NexusStack.Core;

namespace NexusStack.Gateway.Controllers
{
    /// <summary>
    /// 基础服务控制器基类，继承自公共服务的控制器基类
    /// </summary>
    [Route("api/gateway/[controller]")]
    public class BaseController : ApiControllerBase
    {

    }
}
