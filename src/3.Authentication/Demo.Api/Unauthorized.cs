using Microsoft.AspNetCore.SignalR;

namespace Demo.Api;

public class Unauthorized(string message)
    : HubException(message)
{ }
