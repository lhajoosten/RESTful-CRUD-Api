namespace CrudApi.Api.Configuration;

public class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
    public bool AllowCredentials { get; set; }
}

public class RateLimitSettings
{
    public int PermitLimit { get; set; } = 100;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan ReplenishmentPeriod { get; set; } = TimeSpan.FromSeconds(10);
    public int QueueLimit { get; set; } = 10;
}

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
