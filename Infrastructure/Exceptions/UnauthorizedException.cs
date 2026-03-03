using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Exceptions
{
    /// <summary>
    /// 未认证异常
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base("Unauthorized access.")
        {
        }

        public UnauthorizedException(string message) : base(message)
        {
        }

        public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
