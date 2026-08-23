using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Domain.Exceptions;

namespace Shared.Application.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            LogException(exception);
            await HandleExceptionAsync(context, exception);
        }
    }

    private void LogException(Exception exception)
    {
        if (exception is DomainException)
            _logger.LogWarning(exception, "A business rule exception occurred.");
        else
            _logger.LogError(exception, "An unhandled exception occurred.");
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case NotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = exception.Message;
                break;

            case ValidationException validationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "One or more validation errors occurred";
                response.Errors = validationException.Errors;
                break;

            case ConflictException:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.Message = exception.Message;
                break;

            case BusinessRuleException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = exception.Message;
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Message = "Unauthorized access";
                break;

            case KeyNotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Resource not found";
                break;

            case ArgumentException or ArgumentNullException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Invalid argument";
                break;

            case InvalidOperationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = exception.Message;
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = "An unexpected error occurred";
                break;
        }

        response.Error = context.Response.StatusCode == StatusCodes.Status500InternalServerError
            ? null
            : exception.Message;

        return context.Response.WriteAsJsonAsync(response);
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
