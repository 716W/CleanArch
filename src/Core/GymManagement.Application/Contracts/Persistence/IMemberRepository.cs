using GymManagement.Domain.Entities;

namespace GymManagement.Application.Contracts.Persistence;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Member>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Member> AddAsync(Member member, CancellationToken cancellationToken = default);
    Task UpdateAsync(Member member, CancellationToken cancellationToken = default);
    Task DeleteAsync(Member member, CancellationToken cancellationToken = default);
}
