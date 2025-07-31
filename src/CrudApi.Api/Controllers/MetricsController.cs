using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using CrudApi.Application.Interfaces;
using System.Diagnostics;

namespace CrudApi.Api.Controllers;

/// <summary>
/// Metrics Controller - API performance and usage metrics
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize] // Require authentication for metrics
public class MetricsController : BaseApiController
{
    private readonly IProductService _productService;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(IProductService productService, ILogger<MetricsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get API performance metrics
    /// </summary>
    /// <returns>Performance metrics</returns>
    /// <response code="200">Returns performance metrics</response>
    /// <response code="401">If user is not authenticated</response>
    [HttpGet("performance")]
    [ProducesResponseType(typeof(PerformanceMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<PerformanceMetrics> GetPerformanceMetrics()
    {
        var process = Process.GetCurrentProcess();
        
        var metrics = new PerformanceMetrics
        {
            CpuUsage = GetCpuUsage(),
            MemoryUsage = new MemoryMetrics
            {
                WorkingSet = process.WorkingSet64,
                PrivateMemory = process.PrivateMemorySize64,
                VirtualMemory = process.VirtualMemorySize64,
                GCTotalMemory = GC.GetTotalMemory(false)
            },
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount,
            UpTime = DateTime.UtcNow - process.StartTime,
            GCCollections = new GCMetrics
            {
                Gen0 = GC.CollectionCount(0),
                Gen1 = GC.CollectionCount(1),
                Gen2 = GC.CollectionCount(2)
            },
            Timestamp = DateTime.UtcNow
        };

        return Ok(metrics);
    }

    /// <summary>
    /// Get business metrics
    /// </summary>
    /// <returns>Business metrics</returns>
    /// <response code="200">Returns business metrics</response>
    /// <response code="401">If user is not authenticated</response>
    [HttpGet("business")]
    [ProducesResponseType(typeof(BusinessMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BusinessMetrics>> GetBusinessMetrics(CancellationToken cancellationToken = default)
    {
        var totalProducts = await _productService.CountAsync(cancellationToken);
        var activeProducts = await _productService.GetActiveProductsAsync(cancellationToken);
        var lowStockProducts = await _productService.GetLowStockProductsAsync(10, cancellationToken);

        var metrics = new BusinessMetrics
        {
            TotalProducts = totalProducts,
            ActiveProducts = activeProducts.Count(),
            InactiveProducts = totalProducts - activeProducts.Count(),
            LowStockProducts = lowStockProducts.Count(),
            Categories = await GetCategoryMetrics(cancellationToken),
            Timestamp = DateTime.UtcNow
        };

        return Ok(metrics);
    }

    /// <summary>
    /// Get API usage statistics
    /// </summary>
    /// <returns>Usage statistics</returns>
    /// <response code="200">Returns usage statistics</response>
    /// <response code="401">If user is not authenticated</response>
    [HttpGet("usage")]
    [ProducesResponseType(typeof(UsageMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UsageMetrics> GetUsageMetrics()
    {
        // In a real application, you would track these metrics
        // using a metrics library like Application Insights, Prometheus, etc.
        var metrics = new UsageMetrics
        {
            TotalRequests = GetTotalRequests(),
            RequestsPerMinute = GetRequestsPerMinute(),
            AverageResponseTime = GetAverageResponseTime(),
            ErrorRate = GetErrorRate(),
            ActiveConnections = GetActiveConnections(),
            TopEndpoints = GetTopEndpoints(),
            Timestamp = DateTime.UtcNow
        };

        return Ok(metrics);
    }

    private async Task<List<CategoryMetric>> GetCategoryMetrics(CancellationToken cancellationToken)
    {
        // This is a simplified implementation
        // In a real application, you might have a dedicated method for this
        var allProducts = await _productService.GetAllAsync(cancellationToken);
        
        return allProducts
            .GroupBy(p => p.Category)
            .Select(g => new CategoryMetric
            {
                Category = g.Key,
                ProductCount = g.Count(),
                AveragePrice = g.Average(p => p.Price),
                TotalStock = g.Sum(p => p.StockQuantity)
            })
            .ToList();
    }

    private double GetCpuUsage()
    {
        // This is a simplified CPU usage calculation
        // In production, you might use a more sophisticated approach
        return Random.Shared.NextDouble() * 100; // Placeholder
    }

    private long GetTotalRequests() => Random.Shared.NextInt64(10000, 100000); // Placeholder
    private double GetRequestsPerMinute() => Random.Shared.NextDouble() * 1000; // Placeholder
    private double GetAverageResponseTime() => Random.Shared.NextDouble() * 500; // Placeholder
    private double GetErrorRate() => Random.Shared.NextDouble() * 5; // Placeholder
    private int GetActiveConnections() => Random.Shared.Next(10, 100); // Placeholder

    private List<EndpointMetric> GetTopEndpoints()
    {
        // Placeholder data - in real app, this would come from metrics collection
        return new List<EndpointMetric>
        {
            new() { Endpoint = "GET /api/v1/products", RequestCount = 1500, AverageResponseTime = 120 },
            new() { Endpoint = "POST /api/v1/products", RequestCount = 200, AverageResponseTime = 250 },
            new() { Endpoint = "GET /api/v1/products/{id}", RequestCount = 800, AverageResponseTime = 80 },
            new() { Endpoint = "PUT /api/v1/products/{id}", RequestCount = 150, AverageResponseTime = 300 },
            new() { Endpoint = "DELETE /api/v1/products/{id}", RequestCount = 50, AverageResponseTime = 100 }
        };
    }
}

// DTOs for metrics
public class PerformanceMetrics
{
    public double CpuUsage { get; set; }
    public MemoryMetrics MemoryUsage { get; set; } = new();
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public TimeSpan UpTime { get; set; }
    public GCMetrics GCCollections { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class MemoryMetrics
{
    public long WorkingSet { get; set; }
    public long PrivateMemory { get; set; }
    public long VirtualMemory { get; set; }
    public long GCTotalMemory { get; set; }
}

public class GCMetrics
{
    public int Gen0 { get; set; }
    public int Gen1 { get; set; }
    public int Gen2 { get; set; }
}

public class BusinessMetrics
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int InactiveProducts { get; set; }
    public int LowStockProducts { get; set; }
    public List<CategoryMetric> Categories { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class CategoryMetric
{
    public string Category { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public decimal AveragePrice { get; set; }
    public int TotalStock { get; set; }
}

public class UsageMetrics
{
    public long TotalRequests { get; set; }
    public double RequestsPerMinute { get; set; }
    public double AverageResponseTime { get; set; }
    public double ErrorRate { get; set; }
    public int ActiveConnections { get; set; }
    public List<EndpointMetric> TopEndpoints { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class EndpointMetric
{
    public string Endpoint { get; set; } = string.Empty;
    public long RequestCount { get; set; }
    public double AverageResponseTime { get; set; }
}
