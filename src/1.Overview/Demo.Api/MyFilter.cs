using Microsoft.AspNetCore.SignalR;

namespace Demo.Api;

public class MyFilter(ILogger<MyFilter> Logger) : IHubFilter
{
    private readonly ILogger<MyFilter> _logger = Logger;

    public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        _logger.LogInformation($"[HUB][FILTER] {invocationContext.HubMethod} before invoke");

        var result = await next(invocationContext);

        _logger.LogInformation($"[HUB][FILTER] {invocationContext.HubMethod} after invoke");

        return result;
    }

    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        _logger.LogInformation($"[HUB][FILTER] {context.Context.ConnectionId} before connect");

        var result = next(context);

        _logger.LogInformation($"[HUB][FILTER] {context.Context.ConnectionId} after connect");

        return result;
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        _logger.LogInformation($"[HUB][FILTER] {context.Context.ConnectionId} before desconnect");

        var result = next(context, exception);

        _logger.LogInformation($"[HUB][FILTER] {context.Context.ConnectionId} after desconnect");

        return result;
    }
}
