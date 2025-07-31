using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using CrudApi.Api.Configuration;
using System.ComponentModel.DataAnnotations;

namespace CrudApi.Api.Controllers;

/// <summary>
/// Authentication Controller - Manages JWT tokens
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Login and get JWT token
    /// </summary>
    /// <param name="loginRequest">Login credentials</param>
    /// <returns>JWT token</returns>
    /// <response code="200">Returns the JWT token</response>
    /// <response code="401">If credentials are invalid</response>
    /// <response code="400">If request data is invalid</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // In a real application, you would validate against a database
        // This is a simplified example
        if (await ValidateCredentialsAsync(loginRequest.Username, loginRequest.Password))
        {
            var token = GenerateJwtToken(loginRequest.Username);
            _logger.LogInformation("User {Username} logged in successfully", loginRequest.Username);
            
            return Ok(new LoginResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                TokenType = "Bearer"
            });
        }

        _logger.LogWarning("Failed login attempt for user {Username}", loginRequest.Username);
        return Unauthorized("Invalid credentials");
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <param name="refreshRequest">Refresh token request</param>
    /// <returns>New JWT token</returns>
    /// <response code="200">Returns the new JWT token</response>
    /// <response code="401">If refresh token is invalid</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest refreshRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // In a real application, you would validate the refresh token against a database
        // This is a simplified example
        var principal = GetPrincipalFromExpiredToken(refreshRequest.Token);
        if (principal?.Identity?.Name == null)
        {
            return Unauthorized("Invalid token");
        }

        var newToken = GenerateJwtToken(principal.Identity.Name);
        _logger.LogInformation("Token refreshed for user {Username}", principal.Identity.Name);

        return Ok(new LoginResponse
        {
            Token = newToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
            TokenType = "Bearer"
        });
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user details</returns>
    /// <response code="200">Returns user information</response>
    /// <response code="401">If user is not authenticated</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UserInfo> GetCurrentUser()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        return Ok(new UserInfo
        {
            Username = username,
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }

    private async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        // In a real application, you would:
        // 1. Hash the password
        // 2. Query the database for the user
        // 3. Compare hashed passwords
        // 4. Check if user is active/enabled
        
        // This is a simplified example for demonstration
        await Task.Delay(100); // Simulate database call
        return username == "admin" && password == "admin123" ||
               username == "user" && password == "user123";
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
        
        if (string.IsNullOrEmpty(jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("username", username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
        
        if (string.IsNullOrEmpty(jwtSettings.SecretKey))
        {
            return null;
        }

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateLifetime = false // We're validating an expired token
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        return principal;
    }

    private int GetJwtExpirationMinutes()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
        return jwtSettings.ExpirationMinutes;
    }
}

// DTOs for authentication
public class LoginRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

public class UserInfo
{
    public string Username { get; set; } = string.Empty;
    public object Claims { get; set; } = new { };
}
