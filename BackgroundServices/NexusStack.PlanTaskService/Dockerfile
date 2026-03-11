FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

COPY publish/ ./
ENTRYPOINT ["dotnet", "NexusStack.PlanTaskService.dll"]