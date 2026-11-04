using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GymManagment.Blazor;
using GymManagement.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Default HttpClient for local static assets (e.g., sample-data/weather.json)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// NSwag API Client configuration
builder.Services.AddScoped<IClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiBaseUrl = configuration["Api:BaseUrl"]
        ?? configuration["ApiBaseUrl"]
        ?? "http://localhost:5062";

    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(apiBaseUrl)
    };

    return new Client(apiBaseUrl, httpClient);
});

await builder.Build().RunAsync();
