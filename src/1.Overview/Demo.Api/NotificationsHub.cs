using Microsoft.AspNetCore.SignalR;

namespace Demo.Api;

public interface INotificationsHub
{
    Task Notify(string notification);
}

public class NotificationsHub(ILogger<NotificationsHub> Logger) : Hub<INotificationsHub>
{
    private readonly ILogger<NotificationsHub> _logger = Logger;

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"[HUB][NOTIFICATIONS] {Context.ConnectionId} connected");

        await Groups.AddToGroupAsync(Context.ConnectionId, "test-group");

        var hub = this as Hub;

        await hub.Clients.Caller.SendAsync("my-notify", "Hello new client!!!!");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if(exception is null)
        {
            _logger.LogInformation($"[HUB][NOTIFICATIONS] {Context.ConnectionId} disconnected");
        }
        else
        {
            _logger.LogError($"[HUB][NOTIFICATIONS] {Context.ConnectionId} disconnected with error: {exception.Message}");
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "test-group");

        await base.OnDisconnectedAsync(exception);
    }


    public Task Ping(string message)
    {
        _logger.LogInformation($"[HUB][NOTIFICATIONS][PING] {Context.ConnectionId} pinged. {message}");

        return Clients.Caller.Notify($"Pong: '{message}'");
    }

    [HubMethodName("my-message")]
    public MyResponse MyMessage(string message)
    {
        _logger.LogInformation($"[HUB][NOTIFICATIONS][MY MESSAGE] {Context.ConnectionId} pinged. {message}");

        return new MyResponse
        {
            FromClient = message,
            CreatedAt = DateTime.UtcNow
        };
    }
}


public record MyResponse
{
    public string? FromClient { get; init; }
    public DateTime CreatedAt { get; init; }
}
