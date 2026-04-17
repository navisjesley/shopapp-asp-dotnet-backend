namespace UserAPI.Contracts;

public sealed class UserBootstrapResponse
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}