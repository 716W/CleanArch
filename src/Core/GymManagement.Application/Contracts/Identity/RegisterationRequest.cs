namespace GymManagement.Application.Contracts.Identity;

public sealed record RegistrationRequest(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string Password);