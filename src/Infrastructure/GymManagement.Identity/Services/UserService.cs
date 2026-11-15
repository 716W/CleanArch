using GymManagement.Application.Contracts.Identity;
using GymManagement.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Identity.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto(
            Id: user.Id,
            FirstName: user.FirstName,
            LastName: user.LastName,
            UserName: user.UserName ?? string.Empty,
            Email: user.Email ?? string.Empty,
            Roles: roles.ToList().AsReadOnly());
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        var dtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            dtos.Add(new UserDto(
                Id: user.Id,
                FirstName: user.FirstName,
                LastName: user.LastName,
                UserName: user.UserName ?? string.Empty,
                Email: user.Email ?? string.Empty,
                Roles: roles.ToList().AsReadOnly()));
        }

        return dtos.AsReadOnly();
    }
}
