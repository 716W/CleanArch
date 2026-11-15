namespace GymManagement.Blazor.Services.Base;

/// <summary>
/// Represents an error response received from the GymManagement Web API.
///
/// Feature services throw this exception when the generated NSwag client's
/// <see cref="Generated.ApiException"/> is caught and translated by
/// <see cref="BaseApiService"/>. Pages catch this type to display user-friendly
/// error messages without depending on the generated exception hierarchy.
/// </summary>
public sealed class GymManagementApiException : Exception
{
    /// <summary>
    /// The HTTP status code returned by the API (e.g., 400, 404, 409).
    /// Zero indicates a network-level failure (no HTTP response received).
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// A short, human-readable title describing the error (from ProblemDetails.Title).
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// An optional detailed explanation of the error (from ProblemDetails.Detail).
    /// </summary>
    public string? Detail { get; }

    /// <summary>
    /// Field-level validation errors keyed by property name, present only on HTTP 400
    /// responses that include a <c>ValidationProblemDetails</c> body.
    /// </summary>
    public IReadOnlyDictionary<string, ICollection<string>>? ValidationErrors { get; }

    /// <summary>Convenience property: true when <see cref="StatusCode"/> is 404.</summary>
    public bool IsNotFound => StatusCode == 404;

    /// <summary>Convenience property: true when <see cref="StatusCode"/> is 400.</summary>
    public bool IsValidationError => StatusCode == 400;

    /// <summary>Convenience property: true when <see cref="StatusCode"/> is 409.</summary>
    public bool IsConflict => StatusCode == 409;

    public GymManagementApiException(
        int statusCode,
        string title,
        string? detail = null,
        IDictionary<string, ICollection<string>>? validationErrors = null,
        Exception? innerException = null)
        : base(BuildMessage(statusCode, title, detail), innerException)
    {
        StatusCode = statusCode;
        Title = title;
        Detail = detail;
        ValidationErrors = validationErrors != null
            ? new Dictionary<string, ICollection<string>>(validationErrors)
            : null;
    }

    private static string BuildMessage(int statusCode, string title, string? detail)
        => detail is not null
            ? $"HTTP {statusCode}: {title} — {detail}"
            : $"HTTP {statusCode}: {title}";
}

