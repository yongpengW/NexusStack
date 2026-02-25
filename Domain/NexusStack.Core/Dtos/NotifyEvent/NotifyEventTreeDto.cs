using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.NotifyEvent
{
    public class NotifyEventTreeDto : NotifyEventDto
    {
        /// <summary>
        /// 下级
        /// </summary>
        public List<NotifyEventTreeDto> Children { get; set; } = new List<NotifyEventTreeDto>();
    }
}
