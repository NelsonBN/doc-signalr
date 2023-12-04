using Microsoft.AspNetCore.SignalR.Client;
using static System.Console;

WriteLine("Starting...");

var counter = 0;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:8080/my-hub")
    .WithAutomaticReconnect([
        TimeSpan.FromMilliseconds(0),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(1_000)])
    .Build();

connection.Reconnecting += (exception) =>
{
    WriteLine($"[RECONNECTING] {nameof(exception)}: {exception}");
    return Task.CompletedTask;
};

connection.Reconnected += (connectionId) =>
{
    WriteLine($"[RECONNECTED] {nameof(connectionId)}: {connectionId}");
    return Task.CompletedTask;
};

connection.Closed += (exception) =>
{
    WriteLine($"[CLOSED] {nameof(exception)}: {exception}");
    return Task.CompletedTask;
};

connection.On<object>("Pong", (data)
    => WriteLine($"[RETURN MESSAGE][{DateTime.UtcNow:HH:mm:ss.ffffff}] {data}"));

try
{
    await connection.StartAsync();

    await SendMessage(connection, counter);

    WriteLine("[CONNECTION] Startined");
    ReadLine();
}
catch(Exception exception)
{
    WriteLine($"[CONNECTION][ERROR] {nameof(exception)}: {exception}");
}


static async Task SendMessage(HubConnection hub, int counter)
{
    while(true)
    {
        var message = $"My message '{counter++}'";

        await hub.SendAsync("Ping", message);

        WriteLine($"\t[SEND MESSAGE] Waiting for connection...");
        await Task.Delay(1_000);
    }
}
