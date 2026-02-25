using NexusStack.Core;
using NexusStack.Infrastructure.Enums;

var moduleKey = "nexusstack-gateway";
var moduleTitle = "NexusStack Gateway";

var builder = WebApplication.CreateBuilder(args);

await builder.InitAppliation(moduleKey, moduleTitle, CoreServiceType.Gateway);