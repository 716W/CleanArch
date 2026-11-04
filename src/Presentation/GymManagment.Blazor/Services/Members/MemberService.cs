using GymManagement.Blazor.Contracts;
using GymManagement.Blazor.Generated;
using GymManagement.Blazor.Services.Base;
using Microsoft.Extensions.Logging;

namespace GymManagement.Blazor.Services.Members;

/// <summary>
/// Implements <see cref="IMemberService"/> by delegating to the generated <see cref="IClient"/>.
///
/// Responsibility: Translate the raw generated-client interface into a stable, page-friendly
/// contract. All API calls are wrapped via <see cref="BaseApiService.ExecuteAsync{T}"/> so that:
///   – <see cref="Generated.ApiException"/> is never leaked to Blazor pages.
///   – Every failure is logged and re-thrown as <see cref="GymManagementApiException"/>.
///   – Return types use <see cref="IReadOnlyList{T}"/> instead of <see cref="ICollection{T}"/>
///     to give pages a read-only, intention-revealing surface.
/// </summary>
public sealed class MemberService : BaseApiService, IMemberService
{
    private readonly IClient _client;

    public MemberService(IClient client, ILogger<MemberService> logger)
        : base(logger)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MemberResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(
            () => _client.MemberAllAsync(cancellationToken));

        // NSwag generates ICollection<T>; we expose the more restrictive IReadOnlyList<T>.
        return result.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public Task<MemberResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(() => _client.MemberGETAsync(id, cancellationToken));

    /// <inheritdoc />
    public Task<MemberResponse> CreateAsync(
        CreateMemberCommand command,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(() => _client.MemberPOSTAsync(command, cancellationToken));

    /// <inheritdoc />
    public Task<MemberResponse> UpdateAsync(
        Guid id,
        UpdateMemberCommand command,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(() => _client.MemberPUTAsync(id, command, cancellationToken));

    /// <inheritdoc />
    public Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(() => _client.MemberDELETEAsync(id, cancellationToken));
}

