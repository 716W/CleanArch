namespace GymManagement.Application.Contracts.Identity;

public sealed record AuthRequest(
    string Email,
    string Password);
