using Microsoft.AspNetCore.Builder;
using NexusStack.Core;
using NexusStack.Infrastructure.Enums;

var moduleKey = "nexusstack-plantask";
var moduleTitle = "NexusStack Plan Task Service";

var builder = WebApplication.CreateBuilder(args);

await builder.InitAppliation(moduleKey, moduleTitle, CoreServiceType.PlanTaskService, true);