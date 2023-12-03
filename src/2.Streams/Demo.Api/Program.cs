using System.Runtime.CompilerServices;
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

app.MapHub<StreamingHub>("/stream");

await app.RunAsync();




public class StreamingHub(ILogger<StreamingHub> Logger) : Hub
{
    private readonly ILogger<StreamingHub> _logger = Logger;

    private readonly Guid _id = Guid.NewGuid();

    public async IAsyncEnumerable<string> Download(Request request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for(var i = 0; i < request.Count; i++)
        {
            if(cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var message = $"[{_id}][{i}] {request.Message}";

            _logger.LogInformation($"[HUB][DOWNLOAD] {message}");

            yield return message;

            await Task.Delay(250, cancellationToken);
        }
    }

    public async Task Upload(IAsyncEnumerable<Request> request)
    {
        await foreach(var data in request)
        {
            _logger.LogInformation($"[HUB][UPLOAD][{_id}][{data.Count}] {data.Message}");
        }
    }
}

public class Request
{
    public int Count { get; set; }
    public required string Message { get; set; }
}
