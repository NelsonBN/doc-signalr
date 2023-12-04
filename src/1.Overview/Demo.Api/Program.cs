using Demo.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

builder.Services.AddSignalR(option => option.AddFilter<MyFilter>());

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials()));

var app = builder.Build();

app.UseSwagger()
   .UseSwaggerUI();

app.UseCors();

app.MapHub<NotificationsHub>("/notifications");


app.MapPost("broadcast", async (IHubContext<NotificationsHub> hub, [FromBody] string message)
    => await hub.Clients.All.SendAsync("onMessage", message));

app.MapPost("group-message", (IHubContext<NotificationsHub, INotificationsHub> hub, [FromBody] string message)
    => hub.Clients
        .Group("test-group")
        .Notify($"Message from group (test-group): {message}"));

await app.RunAsync();
