using GymManagement.Blazor.Generated;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace GymManagement.Blazor.Services.Base;

/// <summary>
/// Shared base for all feature API services.
///
/// Responsibility: Provides a single <see cref="ExecuteAsync{T}"/> and
/// <see cref="ExecuteAsync"/> helper that:
///   1. Calls the generated NSwag client method.
///   2. Catches <see cref="ApiException"/> variants thrown by the generated code.
///   3. Wraps them in a <see cref="GymManagementApiException"/> with clean, page-friendly
///      properties (StatusCode, Title, ValidationErrors).
///   4. Logs every failure through the injected <see cref="ILogger"/>.
///
/// Pages never depend on this class directly. It is an internal implementation detail
/// of the feature services (MemberService, MembershipPlanService, MembershipService).
/// </summary>
public abstract class BaseApiService
{
    private readonly ILogger _logger;

    protected BaseApiService(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executes an API call that returns a result, translating any <see cref="ApiException"/>
    /// into a <see cref="GymManagementApiException"/>.
    /// </summary>
    /// <typeparam name="T">The expected return type of the API call.</typeparam>
    /// <param name="apiCall">A delegate that calls the generated NSwag client method.</param>
    /// <param name="operation">Automatically filled with the calling method name for log context.</param>
    protected async Task<T> ExecuteAsync<T>(
        Func<Task<T>> apiCall,
        [CallerMemberName] string operation = "")
    {
        try
        {
            return await apiCall();
        }
        catch (ApiException<ValidationProblemDetails> ex)
        {
            _logger.LogWarning(
                ex,
                "Validation failure in {Operation}: {Title}",
                operation,
                ex.Result?.Title);

            throw new GymManagementApiException(
                statusCode: ex.StatusCode,
                title: ex.Result?.Title ?? "Validation failed",
                detail: ex.Result?.Detail,
                validationErrors: ex.Result?.Errors);
        }
        catch (ApiException<ProblemDetails> ex)
        {
            _logger.LogError(
                ex,
                "API error in {Operation} (HTTP {StatusCode}): {Title}",
                operation,
                ex.StatusCode,
                ex.Result?.Title);

            throw new GymManagementApiException(
                statusCode: ex.StatusCode,
                title: ex.Result?.Title ?? "An API error occurred",
                detail: ex.Result?.Detail);
        }
        catch (ApiException ex)
        {
            _logger.LogError(
                ex,
                "Untyped API exception in {Operation} (HTTP {StatusCode})",
                operation,
                ex.StatusCode);

            throw new GymManagementApiException(
                statusCode: ex.StatusCode,
                title: "An API error occurred",
                detail: ex.Response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error in {Operation}", operation);

            throw new GymManagementApiException(
                statusCode: 0,
                title: "Network error",
                detail: "Could not reach the API. Please check your connection.");
        }
    }

    /// <summary>
    /// Executes an API call that returns no result (e.g., DELETE, PATCH with 204 response),
    /// translating any <see cref="ApiException"/> into a <see cref="GymManagementApiException"/>.
    /// </summary>
    /// <param name="apiCall">A delegate that calls the generated NSwag client method.</param>
    /// <param name="operation">Automatically filled with the calling method name for log context.</param>
    protected async Task ExecuteAsync(
        Func<Task> apiCall,
        [CallerMemberName] string operation = "")
    {
        // Delegate to the generic overload by wrapping the void call.
        await ExecuteAsync<object?>(async () =>
        {
            await apiCall();
            return null;
        }, operation);
    }
}

