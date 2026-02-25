namespace NexusStack.Infrastructure.Attributes;

/// <summary>
/// SignalR Hub映射特性，用于自动映射Hub端点
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SignalRHubAttribute : Attribute
{
    /// <summary>
    /// Hub端点路径
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="route">Hub端点路径，如 "/hubs/notification"</param>
    public SignalRHubAttribute(string route)
    {
        Route = route;
    }
}