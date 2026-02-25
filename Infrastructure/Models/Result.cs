using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Models
{
    public class Result
    {
        public Result()
        {
            Success = false;
        }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class Result<T>
    {
        public Result()
        {
            Success = false;
        }
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
