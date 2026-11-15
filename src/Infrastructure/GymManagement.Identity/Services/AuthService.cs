using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymManagement.Application.Contracts.Identity;
using GymManagement.Application.Exceptions;
using GymManagement.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GymManagement.Identity.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> LoginAsync(AuthRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new BusinessRuleException("Invalid email or password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            throw new BusinessRuleException("Invalid email or password.");
        }

        var token = await GenerateJwtTokenAsync(user);

        return new AuthResponse(
            Id: user.Id,
            UserName: user.UserName ?? string.Empty,
            Email: user.Email ?? string.Empty,
            Token: token);
    }

    public async Task<RegistrationResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var existingEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            throw new ConflictException($"User with email '{request.Email}' already exists.");
        }

        var existingUsername = await _userManager.FindByNameAsync(request.UserName);
        if (existingUsername != null)
        {
            throw new ConflictException($"Username '{request.UserName}' is already taken.");
        }

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.UserName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BusinessRuleException($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "User");

        return new RegistrationResponse(user.Id);
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
