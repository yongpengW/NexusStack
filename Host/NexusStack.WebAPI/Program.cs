using NexusStack.Core;

var moduleKey = "nexusstack-web-api";
var moduleTitle = "NexusStack Web API";

var builder = WebApplication.CreateBuilder(args);

await builder.InitAppliation(moduleKey, moduleTitle, enableSignalR: true);