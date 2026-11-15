namespace GymManagement.Application.Contracts.Identity;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
