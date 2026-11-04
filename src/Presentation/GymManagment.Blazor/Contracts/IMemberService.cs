using GymManagement.Blazor.Generated;

namespace GymManagement.Blazor.Contracts;

/// <summary>
/// Defines the member-management operations available to Blazor pages.
/// Pages depend on this interface; they never talk to the generated API client directly.
/// </summary>
public interface IMemberService
{
    /// <summary>Retrieves the full list of members.</summary>
    Task<IReadOnlyList<MemberResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single member by its unique identifier.</summary>
    /// <exception cref="GymManagementApiException">Thrown when the API returns an error response.</exception>
    Task<MemberResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Creates a new member.</summary>
    /// <exception cref="GymManagementApiException">Thrown when validation fails or a conflict occurs.</exception>
    Task<MemberResponse> CreateAsync(CreateMemberCommand command, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing member's details.</summary>
    /// <exception cref="GymManagementApiException">Thrown when the member is not found or validation fails.</exception>
    Task<MemberResponse> UpdateAsync(Guid id, UpdateMemberCommand command, CancellationToken cancellationToken = default);

    /// <summary>Permanently deletes a member.</summary>
    /// <exception cref="GymManagementApiException">Thrown when the member is not found.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

