using GymManagement.Blazor.Contracts;
using GymManagement.Blazor.Generated;
using GymManagement.Blazor.Services.Members;
using GymManagement.Blazor.Services.MembershipPlans;
using GymManagement.Blazor.Services.Memberships;
using GymManagment.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ─── Default HttpClient ──────────────────────────────────────────────────────
// Used by Blazor for static-asset requests (e.g., weather.json).
// Not used by the API client, which has its own named HttpClient below.
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// ─── NSwag Generated API Client ─────────────────────────────────────────────
// The API base URL is read from wwwroot/appsettings.json → "Api:BaseUrl".
// The generated Client constructor requires (string baseUrl, HttpClient httpClient).
// A dedicated HttpClient instance is created here so it is completely independent
// of the default Blazor HttpClient registered above.
builder.Services.AddScoped<IClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiBaseUrl = configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException(
               "Missing required configuration key 'Api:BaseUrl' in wwwroot/appsettings.json.");

    var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
    return new Client(apiBaseUrl, httpClient);
});

// ─── Feature Services ────────────────────────────────────────────────────────
// Scoped lifetime is correct for Blazor WASM: each Blazor component tree
// shares one scope per render, and services resolve the same IClient instance
// within that scope.
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMembershipPlanService, MembershipPlanService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();

// ─── Logging ─────────────────────────────────────────────────────────────────
// Blazor WASM registers a default ILogger<T> implementation automatically via
// WebAssemblyHostBuilder. BaseApiService injects ILogger through the concrete
// service's ILogger<T>, so no extra registration is needed.

await builder.Build().RunAsync();
