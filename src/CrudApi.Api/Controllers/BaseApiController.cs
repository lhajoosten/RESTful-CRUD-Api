using Microsoft.AspNetCore.Mvc;

namespace CrudApi.Api.Controllers;

/// <summary>
/// Base API Controller with common functionality
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Creates a consistent error response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="details">Additional error details</param>
    /// <returns>Problem details response</returns>
    protected ActionResult CreateErrorResponse(string message, object? details = null)
    {
        return Problem(
            detail: message,
            instance: HttpContext.Request.Path,
            title: "An error occurred",
            statusCode: StatusCodes.Status500InternalServerError,
            extensions: details != null ? new Dictionary<string, object?> { { "details", details } } : null
        );
    }

    /// <summary>
    /// Creates a consistent validation error response
    /// </summary>
    /// <param name="message">Validation error message</param>
    /// <returns>Bad request response</returns>
    protected ActionResult CreateValidationErrorResponse(string message)
    {
        return Problem(
            detail: message,
            instance: HttpContext.Request.Path,
            title: "Validation error",
            statusCode: StatusCodes.Status400BadRequest
        );
    }

    /// <summary>
    /// Creates a consistent not found response
    /// </summary>
    /// <param name="resourceName">Name of the resource that was not found</param>
    /// <param name="resourceId">ID of the resource that was not found</param>
    /// <returns>Not found response</returns>
    protected ActionResult CreateNotFoundResponse(string resourceName, object resourceId)
    {
        return Problem(
            detail: $"{resourceName} with ID '{resourceId}' was not found",
            instance: HttpContext.Request.Path,
            title: "Resource not found",
            statusCode: StatusCodes.Status404NotFound
        );
    }
}
