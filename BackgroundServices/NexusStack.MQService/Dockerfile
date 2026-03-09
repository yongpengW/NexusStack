FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8081

COPY publish/ ./
ENV ASPNETCORE_URLS http://+:8081
ENTRYPOINT ["dotnet", "NexusStack.MQService.dll"]