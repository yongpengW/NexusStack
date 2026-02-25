using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.OpenApiConfigs
{
    public class OpenApiConfigQueryDto : PagedQueryModelBase
    {
        public string? AppName { get; set; }
    }
}
