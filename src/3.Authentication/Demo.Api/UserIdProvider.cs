using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Demo.Api;

public class UserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
        => connection.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
