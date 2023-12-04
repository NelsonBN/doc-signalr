using Microsoft.AspNetCore.SignalR;

namespace Demo.Api;

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
