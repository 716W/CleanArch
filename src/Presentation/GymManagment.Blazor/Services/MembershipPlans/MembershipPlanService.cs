using GymManagement.Blazor.Contracts;
using GymManagement.Blazor.Generated;
using GymManagement.Blazor.Services.Base;
using Microsoft.Extensions.Logging;

namespace GymManagement.Blazor.Services.MembershipPlans;

/// <summary>
/// Implements <see cref="IMembershipPlanService"/> by delegating to the generated <see cref="IClient"/>.
///
/// Responsibility: Translate the raw generated-client interface into a stable, page-friendly
/// contract. All API calls are wrapped via <see cref="BaseApiService.ExecuteAsync{T}"/> so that:
///   – <see cref="Generated.ApiException"/> is never leaked to Blazor pages.
///   – Every failure is logged and re-thrown as <see cref="GymManagementApiException"/>.
/// </summary>
public sealed class MembershipPlanService : BaseApiService, IMembershipPlanService
{
    private readonly IClient _client;

    public MembershipPlanService(IClient client, ILogger<MembershipPlanService> logger)
        : base(logger)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MembershipPlanResponse>> GetAllAsync(
        bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(
            () => _client.MembershipPlansAllAsync(activeOnly, cancellationToken));

        return result.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public Task<MembershipPlanResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(() => _client.MembershipPlansGETAsync(id, cancellationToken));

    /// <inheritdoc />
    public Task<MembershipPlanResponse> CreateAsync(
        CreateMembershipPlanCommand command,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(() => _client.MembershipPlansPOSTAsync(command, cancellationToken));

    /// <inheritdoc />
    public Task<MembershipPlanResponse> UpdateAsync(
        Guid id,
        UpdateMembershipPlanCommand command,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(() => _client.MembershipPlansPUTAsync(id, command, cancellationToken));

    /// <inheritdoc />
    public Task ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(() => _client.ActivateAsync(id, cancellationToken));

    /// <inheritdoc />
    public Task DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(() => _client.DeactivateAsync(id, cancellationToken));
}

