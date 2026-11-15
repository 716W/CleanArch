namespace GymManagement.Application.Contracts.Identity;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(AuthRequest request, CancellationToken cancellationToken = default);
    Task<RegistrationResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);
}