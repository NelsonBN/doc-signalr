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


public class AuthenticationFilter(ILogger<AuthenticationFilter> Logger) : IHubFilter
{
    private readonly ILogger<AuthenticationFilter> _logger = Logger;
    public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        await Task.Delay(1000);
        var expiry = invocationContext.Context?.User?.Claims?.FirstOrDefault(x => x.Type == "exp")?.Value ?? "0";
        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry));
        if(DateTimeOffset.UtcNow.Subtract(expiryDate) > TimeSpan.Zero)
        {
            _logger.LogError($"[HUB][FILTER] expired token for {invocationContext.HubMethod}");
            throw new Unauthorized("Expired Token!!!");
        }

        return await next(invocationContext);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        _logger.LogInformation($"[HUB][FILTER] {context.Context.ConnectionId} before desconnect");

        var result = next(context, exception);

        _logger.LogInformation($"[HUB][FILTER] {context.Context.ConnectionId} after desconnect");

        return result;
    }
}
