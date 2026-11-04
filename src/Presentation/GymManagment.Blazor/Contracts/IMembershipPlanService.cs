using GymManagement.Blazor.Generated;

namespace GymManagement.Blazor.Contracts;

/// <summary>
/// Defines the membership-plan operations available to Blazor pages.
/// Pages depend on this interface; they never talk to the generated API client directly.
/// </summary>
public interface IMembershipPlanService
{
    /// <summary>
    /// Retrieves all membership plans, optionally filtered to active-only plans.
    /// </summary>
    Task<IReadOnlyList<MembershipPlanResponse>> GetAllAsync(
        bool? activeOnly = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single membership plan by its unique identifier.</summary>
    /// <exception cref="GymManagementApiException">Thrown when the plan is not found.</exception>
    Task<MembershipPlanResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Creates a new membership plan.</summary>
    /// <exception cref="GymManagementApiException">Thrown when validation fails.</exception>
    Task<MembershipPlanResponse> CreateAsync(
        CreateMembershipPlanCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>Updates an existing membership plan.</summary>
    /// <exception cref="GymManagementApiException">Thrown when the plan is not found or validation fails.</exception>
    Task<MembershipPlanResponse> UpdateAsync(
        Guid id,
        UpdateMembershipPlanCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>Activates a membership plan, making it available for new memberships.</summary>
    /// <exception cref="GymManagementApiException">Thrown when the plan is not found.</exception>
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Deactivates a membership plan, preventing it from being used for new memberships.</summary>
    /// <exception cref="GymManagementApiException">Thrown when the plan is not found.</exception>
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

