using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;
using System.Diagnostics;
using Asp.Versioning;

namespace CrudApi.Api.Controllers;

/// <summary>
/// Health Check Controller - System health monitoring
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HealthController : BaseApiController
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Get overall system health status
    /// </summary>
    /// <returns>Health status</returns>
    /// <response code="200">System is healthy</response>
    /// <response code="503">System is unhealthy</response>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthResponse>> GetHealth()
    {
        var healthReport = await _healthCheckService.CheckHealthAsync();
        
        var response = new HealthResponse
        {
            Status = healthReport.Status.ToString(),
            TotalDuration = healthReport.TotalDuration,
            Checks = healthReport.Entries.Select(entry => new HealthCheckResponse
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Duration = entry.Value.Duration,
                Description = entry.Value.Description,
                Exception = entry.Value.Exception?.Message,
                Data = entry.Value.Data
            }).ToList()
        };

        var statusCode = healthReport.Status == HealthStatus.Healthy 
            ? StatusCodes.Status200OK 
            : StatusCodes.Status503ServiceUnavailable;

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Get basic system information
    /// </summary>
    /// <returns>System information</returns>
    /// <response code="200">Returns system information</response>
    [HttpGet("info")]
    [ProducesResponseType(typeof(SystemInfo), StatusCodes.Status200OK)]
    public ActionResult<SystemInfo> GetSystemInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown";
        
        var systemInfo = new SystemInfo
        {
            ApplicationName = "CRUD API",
            Version = version,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            MachineName = Environment.MachineName,
            ProcessorCount = Environment.ProcessorCount,
            OsVersion = Environment.OSVersion.ToString(),
            RuntimeVersion = Environment.Version.ToString(),
            StartTime = Process.GetCurrentProcess().StartTime,
            UpTime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime,
            WorkingSet = GC.GetTotalMemory(false),
            Timestamp = DateTime.UtcNow
        };

        return Ok(systemInfo);
    }

    /// <summary>
    /// Readiness probe for container orchestration
    /// </summary>
    /// <returns>Ready status</returns>
    /// <response code="200">Service is ready</response>
    /// <response code="503">Service is not ready</response>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready()
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();
            return healthReport.Status == HealthStatus.Healthy ? Ok("Ready") : StatusCode(503, "Not Ready");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed during readiness probe");
            return StatusCode(503, "Not Ready");
        }
    }

    /// <summary>
    /// Liveness probe for container orchestration
    /// </summary>
    /// <returns>Alive status</returns>
    /// <response code="200">Service is alive</response>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Live()
    {
        return Ok("Alive");
    }
}

// DTOs for health responses
public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public List<HealthCheckResponse> Checks { get; set; } = new();
}

public class HealthCheckResponse
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? Description { get; set; }
    public string? Exception { get; set; }
    public IReadOnlyDictionary<string, object>? Data { get; set; }
}

public class SystemInfo
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public int ProcessorCount { get; set; }
    public string OsVersion { get; set; } = string.Empty;
    public string RuntimeVersion { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public TimeSpan UpTime { get; set; }
    public long WorkingSet { get; set; }
    public DateTime Timestamp { get; set; }
}
