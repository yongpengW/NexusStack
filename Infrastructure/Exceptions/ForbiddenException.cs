using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Exceptions
{
    /// <summary>
    /// 身份验证异常
    /// </summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException() : base()
        {
        }
        public ForbiddenException(string message) : base(message)
        {
        }
        public ForbiddenException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}
