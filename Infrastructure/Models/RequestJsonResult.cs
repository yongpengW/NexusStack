using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Models
{
    /// <summary>
    /// 继承JsonResult，用于统一返回数据格式
    /// </summary>
    public class RequestJsonResult : JsonResult
    {
        public RequestJsonResult(object value) : base(value)
        {
        }
    }
}
