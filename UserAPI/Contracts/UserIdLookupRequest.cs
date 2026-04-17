namespace UserAPI.Contracts;

public sealed class UserIdLookupRequest
{
    public string Provider { get; set; } = string.Empty;
    public Guid ProviderOId { get; set; }
}