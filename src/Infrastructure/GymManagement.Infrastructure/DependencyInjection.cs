using GymManagement.Application.Contracts.Persistence;
using GymManagement.Application.Contracts.Services;
using GymManagement.Infrastructure.Persistence.Context;
using GymManagement.Infrastructure.Persistence.Repositories;
using GymManagement.Infrastructure.Services.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;

namespace GymManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=gymmanagement.db";

        services.AddDbContext<GymDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IMemberRepository, MemberRepository>();

        services.AddSingleton<ISendGridClient>(_ =>
            new SendGridClient(configuration["SendGrid:ApiKey"])
        );

        services.AddScoped<IEmailService, SendGridEmailService>();

        return services;
    }
}
