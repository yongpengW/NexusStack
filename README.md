# NexusStack - 现代化微服务解决方案

<div align="center">

🚀 **一个现代化的、生产级别的 .NET 微服务解决方案**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![许可证](https://img.shields.io/badge/许可证-MIT-green.svg)](LICENSE)
[![GitHub Stars](https://img.shields.io/github/stars/yongpengW/NexusStack?style=social)](https://github.com/yongpengW/NexusStack)

</div>

## ✨ 特性

> 这是一个基于 .NET 10 的现代化微服务解决方案，旨在帮助开发者快速构建高性能、可扩展的微服务应用程序。它集成了多种流行的技术和工具，提供了一个开箱即用的架构模板，适用于各种规模的项目。
> 简化了微服务开发的复杂性，提供了一个清晰的分层架构和丰富的功能模块，让开发者能够专注于业务逻辑的实现，而无需担心基础设施的搭建和集成。
> PS. 其实主要是为了偷懒，毕竟每次新项目都要搭建一大堆东西，干脆做成模板了。如果本项目也能为你节省时间和精力，那就太好了！

- 🏗️ **清晰架构** - 结构良好的分层（基础设施、领域、宿主）
- 🌐 **API 网关** - 基于 YARP 的动态路由网关，支持实时配置管理和服务代理
- 🔄 **SignalR** - 实时通信，自动映射 Hub
- 📨 **RabbitMQ** - 事件驱动的消息系统
- 💾 **Redis** - 分布式缓存
- 🗄️ **EF Core** - 多数据库支持（PostgreSQL、MySQL、SQL Server）
- ⚙️ **AgileConfig** - 轻量级分布式配置中心
- 📊 **Swagger/OpenAPI** - 自动 API 文档
- 📝 **Serilog + Seq** - 结构化日志与集中式日志查询
- ⏰ **后台任务** - 基于 Cron 的任务调度
- 🐳 **Docker 就绪** - 完整的容器化支持

## 🚀 快速开始

> ⚠️ **重要提示**: 在启动项目前，请先参考 [配置中心 (AgileConfig)](#配置中心-agileconfig) 章节搭建 AgileConfig 配置中心。

```powershell
# 1. 安装模板
.\Install-Template.ps1

# 2. 创建你的项目
dotnet new nexusstack -n MyAwesomeProject

# 3. 运行你的项目
cd MyAwesomeProject
dotnet run --project Host\MyAwesomeProject.WebAPI
```

## 📖 详细指南

- [安装模板](#安装模板)
- [创建项目](#创建项目)
- [配置参数](#配置参数)
- [项目结构](#项目结构)
- [配置中心 (AgileConfig)](#配置中心-agileconfig)
- [开发指南](#开发指南)
- [故障排除](#故障排除)

## 📦 包含内容

```
您的项目/
├── Infrastructure/           # 核心工具和模型
├── Domain/                   # 业务逻辑
│   ├── 您的项目.Core/
│   ├── 您的项目.EFCore/
│   ├── 您的项目.Redis/
│   ├── 您的项目.RabbitMQ/
│   └── ...
├── Host/                     # 应用程序入口
│   ├── 您的项目.WebAPI/    # API 服务
│   └── 您的项目.Gateway/   # API 网关
└── BackgroundServices/       # 后台服务
```

---

## 安装模板

### 方式 1: 使用安装脚本（推荐）
```powershell
.\Install-Template.ps1
```

选项说明：
- **1** - 本地安装（开发测试用）
- **4** - 卸载现有模板
- **5** - 完全清理并重新安装（推荐）

### 方式 2: 命令行安装
```powershell
# 首次安装
dotnet new install .

# 更新安装
dotnet new install . --force
```

### 验证安装
```powershell
# 查看已安装的模板
dotnet new list | Select-String "nexusstack"

# 查看参数
dotnet new nexusstack --help
```

---

## 创建项目

### 基础用法
```powershell
dotnet new nexusstack -n MyProject
```

### 高级配置
```powershell
# 企业级项目（包含网关）
dotnet new nexusstack -n Enterprise.HR.API \
  --DatabaseProvider MySQL \
  --EnableSignalR true \
  --IncludeGateway true \
  --IncludeMQService false

# 最小配置（不包含网关和后台服务）
dotnet new nexusstack -n SimpleAPI \
  --IncludeGateway false \
  --IncludeMQService false \
  --IncludePlanTaskService false

# API + 网关配置
dotnet new nexusstack -n MyMicroservices \
  --IncludeGateway true \
  --EnableSignalR true

# .NET 8 项目
dotnet new nexusstack -n Net8Project \
  --TargetFramework net8.0
```

### 预览文件
```powershell
# 查看会生成哪些文件
dotnet new nexusstack -n MyProject --dry-run
```

---

## 配置参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-n, --name` | string | - | 项目名称（必需） |
| `--EnableSignalR` | bool | true | 启用 SignalR |
| `--EnableRabbitMQ` | bool | true | 启用 RabbitMQ |
| `--EnableRedis` | bool | true | 启用 Redis |
| `--IncludeGateway` | bool | true | 包含 API 网关项目 |
| `--DatabaseProvider` | choice | PostgreSQL | PostgreSQL/MySQL/SqlServer |
| `--IncludeMQService` | bool | true | 包含 MQ 后台服务 |
| `--IncludePlanTaskService` | bool | true | 包含计划任务服务 |
| `--TargetFramework` | choice | net10.0 | net8.0/net9.0/net10.0 |

---

## 项目结构

```
MyProject/
├── MyProject.sln                    # 传统解决方案（命令行/CI）
├── MyProject_Backend.slnx           # 新式解决方案（VS 2022+）
├── Infrastructure/                  # 基础设施层
│   └── MyProject.Infrastructure.csproj
├── Domain/                          # 业务领域层
│   ├── MyProject.Core/             # 核心服务
│   ├── MyProject.EFCore/           # 数据访问
│   ├── MyProject.Excel/            # Excel 操作
│   ├── MyProject.RabbitMQ/         # 消息队列
│   ├── MyProject.Redis/            # 缓存
│   ├── MyProject.Serilog/          # 日志
│   └── MyProject.Swagger/          # API 文档
├── Host/                            # 宿主层
│   ├── MyProject.WebAPI/           # API 服务
│   └── MyProject.Gateway/          # API 网关
└── BackgroundServices/              # 后台服务
    ├── MyProject.MQService/
    └── MyProject.PlanTaskService/
```

### 解决方案文件

- **`.sln`** - 命令行/CI/CD 使用，兼容性最好
- **`.slnx`** - VS 2022+ 使用，更简洁

---

## 配置中心 (AgileConfig)

### 📋 概述

NexusStack 默认使用 **AgileConfig** 作为分布式配置中心。AgileConfig 是一个轻量级的配置管理系统，支持配置的集中管理、动态更新和多环境管理。

> ⚠️ **重要提示**: 项目启动前需要先搭建 AgileConfig 配置中心服务。

### 🚀 快速搭建 AgileConfig

#### 方式 1: Docker 部署（推荐）

```powershell
# 1. 拉取镜像
docker pull kklldog/agile_config:latest

# 2. 运行容器
docker run -d --name agileconfig \
  -p 5000:5000 \
  -e TZ=Asia/Shanghai \
  -e adminConsole=true \
  kklldog/agile_config:latest

# 3. 访问管理控制台
# http://localhost:5000
# 默认账号: admin
# 默认密码: admin
```

#### 方式 2: Docker Compose 部署

创建 `docker-compose.yml` 文件：

```yaml
version: '3.8'
services:
  agileconfig:
    image: kklldog/agile_config:latest
    container_name: agileconfig
    ports:
      - "5000:5000"
    environment:
      - TZ=Asia/Shanghai
      - adminConsole=true
      - db:provider=sqlite  # 或使用 mysql/sqlserver
    volumes:
      - ./agileconfig_data:/app/db
    restart: unless-stopped
```

启动服务：
```powershell
docker-compose up -d
```

### ⚙️ 配置应用程序

#### 1. 在 AgileConfig 控制台创建应用

1. 登录 AgileConfig 管理控制台 (http://localhost:5000)
2. 创建新应用，记录 `AppId` 和 `Secret`
3. 添加配置项（支持 JSON、Text、XML 等格式）

#### 2. 配置项目

在 `appsettings.json` 中配置 AgileConfig 连接信息：

```json
// Host/MyProject.WebAPI/appsettings.json
{
  "AgileConfig": {
    "appId": "your-app-id",           // 在 AgileConfig 控制台创建的应用ID
    "secret": "your-secret",          // 应用密钥
    "nodes": "http://localhost:5000", // AgileConfig 服务地址
    "name": "MyProject",              // 应用名称（可选）
    "tag": "dev",                     // 环境标签（可选）
    "env": "DEV"                      // 环境名称（可选）
  }
}
```

#### 3. 使用配置

配置会自动注入到 `IConfiguration` 中，无需额外代码即可使用：

```csharp
public class MyService
{
    private readonly IConfiguration _configuration;

    public MyService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void UseConfig()
    {
        // 直接读取 AgileConfig 中的配置
        var value = _configuration["YourConfigKey"];
    }
}
```

### 🔄 配置热更新

AgileConfig 支持配置热更新，无需重启服务：

1. 在 AgileConfig 控制台修改配置
2. 点击"发布"按钮
3. 应用程序会自动接收新配置（通常在 5-10 秒内）

### 📝 最佳实践

1. **环境隔离**: 为不同环境（开发、测试、生产）创建不同的应用或使用不同的 tag
2. **敏感信息**: 数据库连接字符串、API密钥等敏感信息统一存放在 AgileConfig
3. **配置分组**: 使用 Group 功能对配置进行分组管理
4. **版本管理**: AgileConfig 支持配置历史记录，可以随时回滚
5. **本地开发**: 开发环境可以在 `appsettings.Development.json` 中覆盖配置，避免连接配置中心

### 🔧 高级配置

#### 禁用 AgileConfig（本地开发）

如果本地开发不想使用配置中心，可以在 `appsettings.Development.json` 中设置：

```json
{
  "AgileConfig": {
    "appId": "",
    "secret": "",
    "nodes": ""
  }
}
```

或直接删除 AgileConfig 配置节点，项目会使用本地配置文件。

#### 使用多个配置节点（高可用）

```json
{
  "AgileConfig": {
    "appId": "your-app-id",
    "secret": "your-secret",
    "nodes": "http://node1:5000,http://node2:5000,http://node3:5000"
  }
}
```

### 📚 更多资源

- [AgileConfig 官方文档](https://github.com/dotnetcore/AgileConfig)
- [AgileConfig 使用教程](https://github.com/dotnetcore/AgileConfig/wiki)
- [配置示例](https://github.com/dotnetcore/AgileConfig/wiki/Configuration-Examples)

---

## 开发指南

### 初始化项目

1. **搭建 AgileConfig 配置中心（必需）**

   参考 [配置中心 (AgileConfig)](#配置中心-agileconfig) 章节先搭建配置中心。

2. **配置 AgileConfig 连接**
```json
// Host/MyProject.WebAPI/appsettings.json
{
  "AgileConfig": {
    "appId": "your-app-id",
    "secret": "your-secret",
    "nodes": "http://localhost:5000"
  }
}
```

3. **配置 Seq 日志（可选）**
```json
// Host/MyProject.WebAPI/appsettings.json
{
  "Seq": {
    "ServerUrl": "http://localhost:5341",
    "ApiKey": "your-api-key"
  }
}
```

4. **配置数据库**
```json
// Host/MyProject.WebAPI/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=mydb;..."
  }
}
```

5. **运行迁移**
```powershell
cd Host/MyProject.WebAPI
dotnet ef database update
```

6. **启动项目**
```powershell
# 启动 API 服务
dotnet run --project Host/MyProject.WebAPI

# 启动 API 网关
dotnet run --project Host/MyProject.Gateway
```

### 网关配置

网关支持动态路由配置，配置文件位于 `Host/MyProject.Gateway/proxy-config.json`

**示例配置：**
```json
{
  "Routes": [
    {
      "RouteId": "api-route",
      "ClusterId": "api-cluster",
      "Match": {
        "Path": "/api/{**catch-all}"
      }
    }
  ],
  "Clusters": {
    "api-cluster": {
      "Destinations": {
        "destination1": {
          "Address": "http://localhost:5000"
        }
      }
    }
  }
}
```

**网关管理 API：**
- `GET /routes` - 获取所有路由
- `POST /routes` - 创建新路由
- `PUT /routes/{routeId}` - 更新路由
- `DELETE /routes/{routeId}` - 删除路由

### 常用命令

```powershell
# 构建
dotnet build

# 运行 API 服务
dotnet run --project Host/MyProject.WebAPI

# 运行网关
dotnet run --project Host/MyProject.Gateway

# 测试
dotnet test

# 发布
dotnet publish -c Release -o ./publish

# 数据库迁移
dotnet ef migrations add InitialCreate --project Domain/MyProject.EFCore
dotnet ef database update --project Host/MyProject.WebAPI
```

### Docker 部署

```powershell
# 构建镜像
docker build -t myproject:latest -f Host/MyProject.WebAPI/Dockerfile .

# 运行
docker run -d -p 5000:8080 myproject:latest
```

---

## 故障排除

### 找不到模板
```powershell
# 解决方案
.\Install-Template.ps1
# 选择: 5 (完全清理并重新安装)
```

### 卸载失败
```powershell
# 查看实际卸载命令
dotnet new uninstall

# 使用显示的命令
dotnet new uninstall <显示的路径>
```

### 编译失败
```powershell
# 还原并重新构建
dotnet restore
dotnet clean
dotnet build
```

---

## 环境要求

- **.NET SDK**: 10.0+
- **Visual Studio**: 2022+ （推荐）
- **数据库**: PostgreSQL 12+ / MySQL 8.0+ / SQL Server 2019+
- **AgileConfig**: （必需）轻量级配置中心 - [快速搭建指南](#配置中心-agileconfig)
- **Seq**: （可选）集中式日志服务器

---

## 🎯 使用场景

适用于:
- 🏢 企业微服务
- 🚀 SaaS 平台
- 📱 移动应用后端
- 🔌 API 网关
- 🛠️ 内部工具

## 🤝 贡献

欢迎贡献！请随时提交 Pull Request。

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 👨‍💻 作者

**Leo Wang**
- GitHub: [@yongpengW](https://github.com/yongpengW)

---

<div align="center">

**⭐ 如果这个项目对你有帮助，请给它一个星标！ ⭐**

由 Leo Wang 用 ❤️ 制作

</div>
