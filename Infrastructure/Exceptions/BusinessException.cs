using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Exceptions
{
    /// <summary>
    /// 业务异常
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message)
        {
        }
        public BusinessException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}
