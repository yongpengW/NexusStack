using NexusStack.Core.Entities.AsyncTasks;
using NexusStack.RabbitMQ.EventBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.EventData
{
    /// <summary>
    /// 异步任务传输数据
    /// </summary>
    public class AsyncTaskEventData : EventBase
    {
        public override string TaskCode => "AsyncTask";
        public AsyncTaskEventData()
        { }

        public AsyncTaskEventData(long id, string data)
        {
            this.TaskId = id;
            this.Data = data;
        }
        public AsyncTaskEventData(AsyncTask asyncTask)
            : this(asyncTask.Id, asyncTask.Data)
        {

        }


        /// <summary>
        /// 异步任务Id
        /// </summary>
        public override long TaskId { get; set; }

        public override string Data { get; set; }

    }
}
