using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Exceptions
{
    /// <summary>
    /// 异常的错误基类，以及添加错误代码
    /// </summary>
    public class ErrorCodeException : Exception
    {
        public int ErrorCode { get; set; }

        public ErrorCodeException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public ErrorCodeException(int errorCode, string message, Exception exception) : base(message, exception)
        {
            ErrorCode = errorCode;
        }
    }
}
