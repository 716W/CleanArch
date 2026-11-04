using System.Diagnostics;
using FluentValidation;
using GymManagement.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Api.Middlewares;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            _logger.LogWarning("The response has already started, cannot execute global exception handler.");
            return false;
        }

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        ProblemDetails problemDetails;

        switch (exception)
        {
            case ValidationException validationException:
            {
                var errors = validationException.Errors
                    .GroupBy(e => e.PropertyName ?? string.Empty)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

                problemDetails = new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = "One or more validation errors occurred.",
                    Instance = httpContext.Request.Path
                };
                break;
            }

            case NotFoundException notFoundException:
            {
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = notFoundException.Message,
                    Instance = httpContext.Request.Path
                };
                break;
            }

            case KeyNotFoundException keyNotFoundException:
            {
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = keyNotFoundException.Message,
                    Instance = httpContext.Request.Path
                };
                break;
            }

            case ConflictException conflictException:
            {
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Detail = conflictException.Message,
                    Instance = httpContext.Request.Path
                };
                break;
            }

            case BusinessRuleException businessRuleException:
            {
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Business Rule Violation",
                    Detail = businessRuleException.Message,
                    Instance = httpContext.Request.Path
                };
                break;
            }

            case DbUpdateException dbUpdateException when IsUniqueConstraintViolation(dbUpdateException):
            {
                _logger.LogWarning(
                    dbUpdateException,
                    "A unique constraint violation occurred while processing the request. TraceId: {TraceId}",
                    traceId);

                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Detail = "A resource with the specified unique value already exists.",
                    Instance = httpContext.Request.Path
                };
                break;
            }

            case BadHttpRequestException badHttpRequestException:
            {
                problemDetails = new ProblemDetails
                {
                    Status = badHttpRequestException.StatusCode,
                    Title = badHttpRequestException.StatusCode == StatusCodes.Status415UnsupportedMediaType
                        ? "Unsupported Media Type"
                        : "Bad Request",
                    Detail = badHttpRequestException.Message,
                    Instance = httpContext.Request.Path
                };
                break;
            }

            case UnauthorizedAccessException unauthorizedAccessException:
            {
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Detail = string.IsNullOrWhiteSpace(unauthorizedAccessException.Message)
                        ? "You do not have permission to access this resource."
                        : unauthorizedAccessException.Message,
                    Instance = httpContext.Request.Path
                };
                break;
            }

            default:
            {
                _logger.LogError(
                    exception,
                    "An unhandled exception occurred while processing the request. TraceId: {TraceId}",
                    traceId);

                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred. Please try again later.",
                    Instance = httpContext.Request.Path
                };
                break;
            }
        }

        problemDetails.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), cancellationToken: cancellationToken);

        return true;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is SqliteException sqliteEx)
        {
            return sqliteEx.SqliteExtendedErrorCode == 2067
                || (sqliteEx.SqliteErrorCode == 19 && sqliteEx.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }
}
