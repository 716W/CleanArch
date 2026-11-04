using GymManagement.Blazor.Generated;

namespace GymManagement.Blazor.Contracts;

/// <summary>
/// Defines the membership operations available to Blazor pages.
/// Pages depend on this interface; they never talk to the generated API client directly.
/// </summary>
public interface IMembershipService
{
    /// <summary>Retrieves a single membership by its unique identifier.</summary>
    /// <exception cref="GymManagementApiException">Thrown when the membership is not found.</exception>
    Task<MembershipResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all memberships belonging to a specific member.</summary>
    /// <exception cref="GymManagementApiException">Thrown when the member is not found.</exception>
    Task<IReadOnlyList<MembershipResponse>> GetByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new membership for a member under a specific plan.</summary>
    /// <exception cref="GymManagementApiException">Thrown when validation fails, the member or plan is not found, or a conflict exists.</exception>
    Task<MembershipResponse> CreateAsync(
        CreateMembershipCommand command,
        CancellationToken cancellationToken = default);
}

