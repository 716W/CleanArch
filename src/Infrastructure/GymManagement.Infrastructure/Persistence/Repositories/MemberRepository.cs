using GymManagement.Application.Contracts.Persistence;
using GymManagement.Domain.Entities;
using GymManagement.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly GymDbContext _context;

    public MemberRepository(GymDbContext context)
    {
        _context = context;
    }

    public async Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Members.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Member>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Members.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Member> AddAsync(Member member, CancellationToken cancellationToken = default)
    {
        await _context.Members.AddAsync(member, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return member;
    }

    public async Task UpdateAsync(Member member, CancellationToken cancellationToken = default)
    {
        _context.Members.Update(member);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Member member, CancellationToken cancellationToken = default)
    {
        _context.Members.Remove(member);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
