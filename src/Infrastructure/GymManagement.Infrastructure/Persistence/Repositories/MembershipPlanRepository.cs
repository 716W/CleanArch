using GymManagement.Application.Contracts.Persistence;
using GymManagement.Domain.Entities;
using GymManagement.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class MembershipPlanRepository : IMembershipPlanRepository
{
    private readonly GymDbContext _context;

    public MembershipPlanRepository(GymDbContext context)
    {
        _context = context;
    }

    public async Task<MembershipPlan?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.MembershipPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<MembershipPlan>> GetAllAsync(
        bool? activeOnly = null,
        CancellationToken ct = default)
    {
        var query = _context.MembershipPlans.AsNoTracking();

        if (activeOnly.HasValue)
            query = query.Where(p => p.IsActive == activeOnly.Value);

        return await query
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<MembershipPlan> AddAsync(MembershipPlan plan, CancellationToken ct = default)
    {
        await _context.MembershipPlans.AddAsync(plan, ct);
        await _context.SaveChangesAsync(ct);
        return plan;
    }

    public async Task UpdateAsync(MembershipPlan plan, CancellationToken ct = default)
    {
        _context.MembershipPlans.Update(plan);
        await _context.SaveChangesAsync(ct);
    }
}