using System.Text.Json;

namespace NexusStack.Core.SignalR
{
    public static class NotificationGroupNames
    {
        public static string User(long userId) => $"user:{userId}";

        public static string Platform(int platformType) => $"platform:{platformType}";
    }

    public sealed class NotificationRelayMessage
    {
        public NotificationTarget? Target { get; set; }

        public string? Event { get; set; }

        public JsonElement Payload { get; set; }
    }

    public sealed class NotificationTarget
    {
        public string? Type { get; set; }

        public string? UserId { get; set; }

        public string? Group { get; set; }
    }
}

