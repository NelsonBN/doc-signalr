using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddSignalR()
    .AddStackExchangeRedis(options =>
    {
        options.Configuration.ClientName = "DemochatApi";
        options.Configuration.EndPoints.Add(builder.Configuration.GetConnectionString("Redis")!);
    });

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials()));

var app = builder.Build();


app.UseCors();

app.MapGet("/i-am", (ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("HTTP Request");

    logger.LogInformation($"[HTTP] At: '{DateTime.UtcNow}', MachineName: '{Environment.MachineName}'");

    return $"I'm {Environment.MachineName}";
});

app.MapHub<ChatHub>("/chat");

await app.RunAsync();



public class ChatHub(ILogger<ChatHub> Logger) : Hub
{
    private readonly ILogger<ChatHub> _logger = Logger;

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"[{Environment.MachineName}][HUB] {Context.ConnectionId} connected");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if(exception is null)
        {
            _logger.LogInformation($"[{Environment.MachineName}][HUB] {Context.ConnectionId} disconnected");
        }
        else
        {
            _logger.LogError($"[{Environment.MachineName}][HUB] {Context.ConnectionId} disconnected with error: {exception.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task Send(string message)
    {
        _logger.LogInformation($"[{Environment.MachineName}][HUB][MESSAGE] At: '{DateTime.UtcNow}', ConnectionId: '{Context.ConnectionId}', Message: '{message}'");

        return Clients.All.SendAsync("OnMessage", $"[{Environment.MachineName}][{DateTime.UtcNow}][{Context.ConnectionId}] {message}");
    }
}
