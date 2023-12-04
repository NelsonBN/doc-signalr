using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials()));


var app = builder.Build();

app.UseCors();

app.MapHub<MyHub>("/my-hub");

await app.RunAsync();


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

        return Clients.All.SendAsync("Pong", $"Pong -> '{message}', At: {DateTime.UtcNow}");
    }
}
