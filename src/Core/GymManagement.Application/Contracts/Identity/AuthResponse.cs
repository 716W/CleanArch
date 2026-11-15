namespace GymManagement.Application.Contracts.Identity;

public sealed record AuthResponse(
    string Id,
    string UserName,
    string Email,
    string Token);