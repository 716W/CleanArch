using GymManagement.Application.Contracts.Persistence;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class MembershipRepository : IMembershipRepository
{
    private readonly GymDbContext _context;

    public MembershipRepository(GymDbContext context)
    {
        _context = context;
    }

    public async Task<Membership> AddAsync(Membership membership, CancellationToken ct = default)
    {
        await _context.Memberships.AddAsync(membership, ct);
        await _context.SaveChangesAsync(ct);

        return membership;
    }

    public async Task<bool> HasConflictingMembershipAsync(Guid memberId, DateTime startDate, CancellationToken ct = default)
    {
        return await _context.Memberships.AnyAsync(
            m => m.MemberId == memberId &&
            (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen) &&
            m.EndDate >= startDate,
            ct
        );
    }

    public async Task<Membership?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await _context.Memberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Membership>> GetByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Memberships
            .AsNoTracking()
            .Where(m => m.MemberId == memberId)
            .OrderByDescending(m => m.StartDate)
            .ToListAsync(cancellationToken);
    }
}