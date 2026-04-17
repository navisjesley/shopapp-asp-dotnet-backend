namespace OrderAPI.Services;

public sealed class UserApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}