/*
PROMPT:
Using a global exception handler that implements RFC 7807 Problem Details specification provides a standardized way to return error information to clients. 
This implementation maps common exceptions to appropriate HTTP status codes and includes structured logging for better observability. 
In development environments, it also includes detailed exception information in the response for easier debugging.    
*/

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Buckeye.Lending.Api.Middleware;

/// <summary>
/// Global exception handler that implements RFC 7807 Problem Details specification.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _environment = environment;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, type, title) = MapExceptionToStatusCode(exception);

        // Log with structured data
        _logger.LogError(
            exception,
            "Unhandled exception occurred. Type: {ExceptionType}, TraceId: {TraceId}",
            exception.GetType().Name,
            httpContext.TraceIdentifier);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = type,
            Title = title,
            Detail = _environment.IsDevelopment() ? exception.Message : "An error occurred processing your request.",
            Instance = httpContext.Request.Path
        };

        // Add trace ID for correlation
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        // Add timestamp
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        // Include exception details in development environment
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;

            if (exception.InnerException is not null)
            {
                problemDetails.Extensions["innerException"] = new
                {
                    type = exception.InnerException.GetType().FullName,
                    message = exception.InnerException.Message
                };
            }
        }

        httpContext.Response.StatusCode = statusCode;

        // Use the Problem Details service to write the response
        await _problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        return true;
    }

    private static (int StatusCode, string Type, string Title) MapExceptionToStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentException or ArgumentNullException => (
                StatusCodes.Status400BadRequest,
                "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                "Bad Request"
            ),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                "Not Found"
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                "Unauthorized"
            ),
            InvalidOperationException => (
                StatusCodes.Status409Conflict,
                "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                "Conflict"
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                "Internal Server Error"
            )
        };
    }
}