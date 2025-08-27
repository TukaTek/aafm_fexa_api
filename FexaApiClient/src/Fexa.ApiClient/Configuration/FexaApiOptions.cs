namespace Fexa.ApiClient.Configuration;

public class FexaApiOptions
{
    public const string SectionName = "FexaApi";
    
    public string BaseUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = "/oauth/token";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
    public bool EnableLogging { get; set; } = true;
    public int TokenRefreshBufferSeconds { get; set; } = 300; // Refresh 5 minutes before expiry
    public int? DefaultUserId { get; set; } // Default user ID for operations like creating work orders
    
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("BaseUrl is required", nameof(BaseUrl));
            
        if (string.IsNullOrWhiteSpace(ClientId))
            throw new ArgumentException("ClientId is required", nameof(ClientId));
            
        if (string.IsNullOrWhiteSpace(ClientSecret))
            throw new ArgumentException("ClientSecret is required", nameof(ClientSecret));
            
        if (TimeoutSeconds <= 0)
            throw new ArgumentException("TimeoutSeconds must be greater than 0", nameof(TimeoutSeconds));
            
        if (MaxRetryAttempts < 0)
            throw new ArgumentException("MaxRetryAttempts cannot be negative", nameof(MaxRetryAttempts));
    }
}