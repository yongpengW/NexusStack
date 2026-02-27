using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NexusStack.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiResource",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    Code = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    GroupName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    RoutePattern = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    NameSpace = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    ControllerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    ActionName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    RequestMethod = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResource", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "AppConfig",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: ""),
                    AppName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    SecretKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    Sessionkey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    AccessToken = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    AccessValidTime = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfig", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "AppEventConfig",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    Method = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    EventCode = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    HookUrl = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEventConfig", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "AppNotificationConfig",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    Phones = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    NoticeUrl = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNotificationConfig", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "AppWebhookConfig",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    Method = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    HookUrl = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppWebhookConfig", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "AsyncTask",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    State = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: ""),
                    Data = table.Column<string>(type: "text", nullable: false, comment: ""),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true, comment: ""),
                    Result = table.Column<string>(type: "text", nullable: true, comment: ""),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsyncTask", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "DownloadItem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    Extension = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, comment: ""),
                    Size = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    bucket = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    StorageType = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    State = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadItem", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "File",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    Size = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    Extension = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, comment: ""),
                    Url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false, comment: ""),
                    Path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false, comment: ""),
                    StorageType = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: ""),
                    Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: ""),
                    OriginalId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    State = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_File", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "GlobalSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: ""),
                    ConfigurationJson = table.Column<string>(type: "text", nullable: true, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalSettings", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "InternalMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageType = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: ""),
                    Body = table.Column<string>(type: "text", nullable: false, comment: ""),
                    Attachments = table.Column<string>(type: "text", nullable: true, comment: ""),
                    Platforms = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalMessage", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    Code = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    ParentId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    PlatformType = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    Icon = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true, comment: ""),
                    IconType = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    ActiveIcon = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true, comment: ""),
                    ActiveIconType = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    Url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true, comment: ""),
                    Order = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    IsExternalLink = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    IdSequences = table.Column<string>(type: "text", nullable: false, comment: ""),
                    SystemId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Menu_Menu_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Menu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "NotifyEvent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    Code = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    ParentId = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    EventType = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    MessageTypes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true, comment: ""),
                    NotifyRoles = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true, comment: ""),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, comment: ""),
                    Order = table.Column<int>(type: "integer", nullable: true, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotifyEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotifyEvent_NotifyEvent_ParentId",
                        column: x => x.ParentId,
                        principalTable: "NotifyEvent",
                        principalColumn: "Id");
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "OperationLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IpAddress = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: ""),
                    LogType = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: ""),
                    OperationContent = table.Column<string>(type: "text", nullable: true, comment: ""),
                    MenuCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: ""),
                    ErrorTracker = table.Column<string>(type: "text", nullable: true, comment: ""),
                    OperationMenu = table.Column<string>(type: "text", nullable: true, comment: ""),
                    Method = table.Column<string>(type: "text", nullable: true, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationLog", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false, comment: ""),
                    Value = table.Column<string>(type: "text", nullable: false, comment: ""),
                    SystemId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "Region",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: ""),
                    ShortName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: ""),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: ""),
                    ParentId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    Level = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    Order = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    IdSequences = table.Column<string>(type: "text", nullable: false, comment: ""),
                    IsEnable = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Region", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: ""),
                    Platforms = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: ""),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    Order = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    IsEnable = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    SystemId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "ScheduleTask",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    Code = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    IsEnable = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    Expression = table.Column<string>(type: "text", nullable: true, comment: ""),
                    NextExecuteTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    LastExecuteTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleTask", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "SeedDataTask",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastWriteTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    Code = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: ""),
                    IsEnable = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    ExecuteTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    ExecuteStatus = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    ConfigPath = table.Column<string>(type: "text", nullable: true, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeedDataTask", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "SMSMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageType = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    BizId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: ""),
                    MessageStatus = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    Recipient = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    Body = table.Column<string>(type: "text", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SMSMessage", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Mobile = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: ""),
                    RealName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: ""),
                    UserName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: ""),
                    NickName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: ""),
                    Password = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    PasswordSalt = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    IsEnable = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    Gender = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    Avatar = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: ""),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false, comment: ""),
                    LastLoginTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    SignatureUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "InternalMessageRecipient",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    RecipientUserId = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalMessageRecipient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalMessageRecipient_InternalMessage_MessageId",
                        column: x => x.MessageId,
                        principalTable: "InternalMessage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "MenuResource",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    ApiResourceId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuResource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuResource_ApiResource_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalTable: "ApiResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuResource_Menu_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    MenuId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    DataRange = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permission_Menu_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Permission_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "ScheduleTaskRecord",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleTaskId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true, comment: ""),
                    ExecuteStartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    ExpressionTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    ExecuteEndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleTaskRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleTaskRecord_ScheduleTask_ScheduleTaskId",
                        column: x => x.ScheduleTaskId,
                        principalTable: "ScheduleTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "UserDepartment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    DepartmentId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDepartment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDepartment_Region_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Region",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDepartment_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    RoleId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRole_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "");

            migrationBuilder.CreateTable(
                name: "UserToken",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: ""),
                    Token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    TokenHash = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, comment: ""),
                    RefreshToken = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: ""),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    IpAddress = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, comment: ""),
                    UserAgent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false, comment: ""),
                    PlatformType = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    LoginMethodType = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    RefreshTokenIsAvailable = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    LoginType = table.Column<int>(type: "integer", nullable: false, comment: ""),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: ""),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true, comment: ""),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserToken_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "");

            migrationBuilder.CreateIndex(
                name: "IX_InternalMessageRecipient_MessageId",
                table: "InternalMessageRecipient",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_ParentId",
                table: "Menu",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuResource_ApiResourceId",
                table: "MenuResource",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuResource_MenuId",
                table: "MenuResource",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_NotifyEvent_ParentId",
                table: "NotifyEvent",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_MenuId",
                table: "Permission",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_RoleId",
                table: "Permission",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTaskRecord_ScheduleTaskId",
                table: "ScheduleTaskRecord",
                column: "ScheduleTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartment_DepartmentId",
                table: "UserDepartment",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartment_UserId",
                table: "UserDepartment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserToken_UserId",
                table: "UserToken",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfig");

            migrationBuilder.DropTable(
                name: "AppEventConfig");

            migrationBuilder.DropTable(
                name: "AppNotificationConfig");

            migrationBuilder.DropTable(
                name: "AppWebhookConfig");

            migrationBuilder.DropTable(
                name: "AsyncTask");

            migrationBuilder.DropTable(
                name: "DownloadItem");

            migrationBuilder.DropTable(
                name: "File");

            migrationBuilder.DropTable(
                name: "GlobalSettings");

            migrationBuilder.DropTable(
                name: "InternalMessageRecipient");

            migrationBuilder.DropTable(
                name: "MenuResource");

            migrationBuilder.DropTable(
                name: "NotifyEvent");

            migrationBuilder.DropTable(
                name: "OperationLog");

            migrationBuilder.DropTable(
                name: "Options");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "ScheduleTaskRecord");

            migrationBuilder.DropTable(
                name: "SeedDataTask");

            migrationBuilder.DropTable(
                name: "SMSMessage");

            migrationBuilder.DropTable(
                name: "UserDepartment");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "UserToken");

            migrationBuilder.DropTable(
                name: "InternalMessage");

            migrationBuilder.DropTable(
                name: "ApiResource");

            migrationBuilder.DropTable(
                name: "Menu");

            migrationBuilder.DropTable(
                name: "ScheduleTask");

            migrationBuilder.DropTable(
                name: "Region");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
