using GymManagement.Blazor.Contracts;
using GymManagement.Blazor.Generated;
using GymManagement.Blazor.Services.Base;
using Microsoft.Extensions.Logging;

namespace GymManagement.Blazor.Services.Memberships;

/// <summary>
/// Implements <see cref="IMembershipService"/> by delegating to the generated <see cref="IClient"/>.
///
/// Responsibility: Translate the raw generated-client interface into a stable, page-friendly
/// contract. All API calls are wrapped via <see cref="BaseApiService.ExecuteAsync{T}"/> so that:
///   – <see cref="Generated.ApiException"/> is never leaked to Blazor pages.
///   – Every failure is logged and re-thrown as <see cref="GymManagementApiException"/>.
/// </summary>
public sealed class MembershipService : BaseApiService, IMembershipService
{
    private readonly IClient _client;

    public MembershipService(IClient client, ILogger<MembershipService> logger)
        : base(logger)
    {
        _client = client;
    }

    /// <inheritdoc />
    public Task<MembershipResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(() => _client.MembershipsGETAsync(id, cancellationToken));

    /// <inheritdoc />
    public async Task<IReadOnlyList<MembershipResponse>> GetByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(
            () => _client.MemberAsync(memberId, cancellationToken));

        return result.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public Task<MembershipResponse> CreateAsync(
        CreateMembershipCommand command,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(() => _client.MembershipsPOSTAsync(command, cancellationToken));
}

