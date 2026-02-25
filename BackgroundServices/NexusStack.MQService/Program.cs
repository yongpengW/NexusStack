using Microsoft.AspNetCore.Builder;
using NexusStack.Core;
using NexusStack.Infrastructure.Enums;

var moduleKey = "nexusstack-mq";
var moduleTitle = "NexusStack MQ Service";

var builder = WebApplication.CreateBuilder(args);

await builder.InitAppliation(moduleKey, moduleTitle, CoreServiceType.MQService, true);