using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using WorkTogetherly.Shared.Auth;
using WorkTogetherly.Shared.Services;
using WorkTogetherly.Shared.Settings;
using WorkTogetherly.Web.Client;
using WorkTogetherly.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// Auth
builder.Services.AddSingleton<ITokenStorage, LocalStorageTokenStorage>();
builder.Services.AddSingleton<TokenStorageService>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthService>());
builder.Services.AddScoped<AppInitializer>();
builder.Services.AddScoped<IAppInitializer>(sp => sp.GetRequiredService<AppInitializer>());
builder.Services.AddTransient<AuthTokenHandler>();

// Add Geolocation esrvice for browser
builder.Services.AddScoped<IGeolocationService, BrowserGeolocationService>();

//Business services
builder.Services.AddScoped<PageTitleService>();
builder.Services.AddScoped<WorkspaceService>();
builder.Services.AddScoped<AmenityService>();
builder.Services.AddScoped<SlotService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<NotificationHubService>();

//Configure API clients
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]!;
builder.Services.AddSingleton(new ApiSettings(apiBaseUrl));

// "Public" client for unauthenticated endpoints (e.g. login, register)
builder.Services.AddHttpClient("Public", client =>
    client.BaseAddress = new Uri(apiBaseUrl + "/"));

// "Auth" client for authenticated endpoints: automatically includes JWT token in requests
builder.Services.AddHttpClient("Auth", client =>
    client.BaseAddress = new Uri(apiBaseUrl + "/"))
    .AddHttpMessageHandler<AuthTokenHandler>();

// Add GeCoding Client
builder.Services.AddHttpClient("Geocoding", client =>
{
    client.BaseAddress = new Uri("https://photon.komoot.io/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("WorkTogetherly/1.0");
});

await builder.Build().RunAsync();
