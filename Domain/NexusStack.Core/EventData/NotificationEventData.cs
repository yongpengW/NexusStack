using NexusStack.Core.Entities.AsyncTasks;
using NexusStack.RabbitMQ.EventBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.EventData
{
    public class NotificationEventData : EventBase
    {
        public override string TaskCode => "Notification";

        public NotificationEventData() { }

        public NotificationEventData(long id, string code, string data)
        {
            this.TaskId = id;
            this.Data = data;
        }

        public NotificationEventData(AsyncTask asyncTask) : this(asyncTask.Id, asyncTask.Code, asyncTask.Data)
        {

        }

        /// <summary>
        /// 异步任务Id
        /// </summary>
        public override long TaskId { get; set; }

        public override string? Data { get; set; }
    }
}
