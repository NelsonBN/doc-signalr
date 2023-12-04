namespace Demo.Api;

public record LoginResult(Guid UserId, string AccessToke, DateTime ValidTo);
