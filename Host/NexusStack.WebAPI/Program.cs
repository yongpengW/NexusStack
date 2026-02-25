using NexusStack.Core;

var moduleKey = "nexusstack_web_api";
var moduleTitle = "NexusStack_Web_API";

var builder = WebApplication.CreateBuilder(args);

await builder.InitAppliation(moduleKey, moduleTitle, enableSignalR: true);