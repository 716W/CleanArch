using GymManagement.Domain.Entities;

namespace GymManagement.Application.Contracts.Persistence;

public interface IMembershipPlanRepository
{
    Task<MembershipPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MembershipPlan>> GetAllAsync(bool? activeOnly = null, CancellationToken ct = default);
    Task<MembershipPlan> AddAsync(MembershipPlan plan, CancellationToken ct = default);
    Task UpdateAsync(MembershipPlan plan, CancellationToken ct = default);
}