namespace Demo.Api;

public record MyResponse
{
    public string? FromClient { get; init; }
    public DateTime CreatedAt { get; init; }
}
