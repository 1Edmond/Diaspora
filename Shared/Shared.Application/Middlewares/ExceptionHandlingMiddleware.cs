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
        catch (Exception exception) when (exception is OperationCanceledException or TaskCanceledException)
        {
            if (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Requête HTTP ou tâche annulée par le client ({Path})", context.Request.Path);
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 499;
                }
                return;
            }

            LogException(exception);
            if (!context.Response.HasStarted)
            {
                await HandleExceptionAsync(context, exception);
            }
        }
        catch (Exception exception)
        {
            if (exception is OperationCanceledException or TaskCanceledException && context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Requête HTTP ou tâche annulée par le client ({Path})", context.Request.Path);
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 499;
                }
                return;
            }

            LogException(exception);
            if (!context.Response.HasStarted)
            {
                await HandleExceptionAsync(context, exception);
            }
        }
    }

    private void LogException(Exception exception)
    {
        if (exception is OperationCanceledException or TaskCanceledException)
            _logger.LogInformation("Task or operation was canceled.");
        else if (exception is FluentValidation.ValidationException or ValidationException)
            _logger.LogWarning("Validation failed: {Message}", exception.Message);
        else if (exception is DomainException)
            _logger.LogWarning(exception, "A business rule exception occurred.");
        else
            _logger.LogError(exception, "An unhandled exception occurred.");
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Success = false
        };

        switch (exception)
        {
            case OperationCanceledException or TaskCanceledException:
                context.Response.StatusCode = context.RequestAborted.IsCancellationRequested ? 499 : StatusCodes.Status408RequestTimeout;
                response.Message = "L'opération a été interrompue ou a expiré.";
                break;

            case NotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = exception.Message;
                break;

            case FluentValidation.ValidationException fluentValidationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Une ou plusieurs erreurs de validation se sont produites.";
                response.Errors = fluentValidationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
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

            case DomainException:
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
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
