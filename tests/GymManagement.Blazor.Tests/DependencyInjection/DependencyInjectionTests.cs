using GymManagement.Blazor.Contracts;
using GymManagement.Blazor.Generated;
using GymManagement.Blazor.Services.Members;
using GymManagement.Blazor.Services.MembershipPlans;
using GymManagement.Blazor.Services.Memberships;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymManagement.Blazor.Tests.DependencyInjection;

public class DependencyInjectionTests
{
    [Fact]
    public void FeatureServices_CanBeRegisteredAndResolvedFromContainer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add configuration
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Api:BaseUrl"] = "http://localhost:5062"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        // Register client mock
        var mockClient = new Mock<IClient>();
        services.AddScoped<IClient>(_ => mockClient.Object);

        // Register feature services as done in Program.cs
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IMembershipPlanService, MembershipPlanService>();
        services.AddScoped<IMembershipService, MembershipService>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Act
        var memberService = scope.ServiceProvider.GetService<IMemberService>();
        var planService = scope.ServiceProvider.GetService<IMembershipPlanService>();
        var membershipService = scope.ServiceProvider.GetService<IMembershipService>();

        // Assert
        Assert.NotNull(memberService);
        Assert.IsType<MemberService>(memberService);

        Assert.NotNull(planService);
        Assert.IsType<MembershipPlanService>(planService);

        Assert.NotNull(membershipService);
        Assert.IsType<MembershipService>(membershipService);
    }
}

