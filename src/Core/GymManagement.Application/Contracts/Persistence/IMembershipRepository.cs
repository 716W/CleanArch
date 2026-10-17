using GymManagement.Domain.Entities;

namespace GymManagement.Application.Contracts.Persistence;

public interface IMembershipRepository
{
    Task<bool> HasConflictingMembershipAsync(Guid memberId, DateTime startDate, CancellationToken ct = default);
    Task<Membership> AddAsync(Membership membership, CancellationToken ct = default);
    Task<Membership?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Membership>> GetByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default);
}