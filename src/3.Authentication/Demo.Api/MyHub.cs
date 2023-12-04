using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Demo.Api;

[Authorize]
public class MyHub(ILogger<MyHub> Logger) : Hub
{
    private readonly ILogger<MyHub> _logger = Logger;

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"[HUB] {Context.ConnectionId} connected. UserId: {Context.UserIdentifier}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if(exception is null)
        {
            _logger.LogInformation($"[HUB] {Context.ConnectionId} disconnected. UserId: {Context.UserIdentifier}");
        }
        else
        {
            _logger.LogError($"[HUB] {Context.ConnectionId} disconnected with error: {exception.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task Ping(string message)
    {
        _logger.LogInformation($"[HUB][PING] {Context.ConnectionId} pinged. {message}");

        return Clients.Caller.SendAsync("Pong", $"Pong -> '{message}', At: {DateTime.UtcNow}");
    }
}
