using System.Text;
using GymManagement.Application.Contracts.Identity;
using GymManagement.Identity.Context;
using GymManagement.Identity.Models;
using GymManagement.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GymManagement.Identity;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration.GetConnectionString("IdentityConnection")
            ?? "Data Source=gymmanagement.db";

        services.AddDbContext<GymIdentityDbContext>(options =>
            options.UseSqlite(
                connectionString,
                b => b.MigrationsHistoryTable("__EFMigrationsHistory_Identity")));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<GymIdentityDbContext>()
        .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var jwtKey = configuration[$"{JwtSettings.SectionName}:Key"]
            ?? "DefaultDevelopmentSecretKeyMustBe32BytesLongMin!";
        var jwtIssuer = configuration[$"{JwtSettings.SectionName}:Issuer"] ?? "GymManagement";
        var jwtAudience = configuration[$"{JwtSettings.SectionName}:Audience"] ?? "GymManagementUser";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
