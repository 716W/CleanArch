namespace GymManagement.Application.Contracts.Identity;

public sealed record UserDto(
    string Id,
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    IReadOnlyList<string> Roles);
