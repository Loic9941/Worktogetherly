using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using WorkTogetherly.Services;
using WorkTogetherly.Shared.Auth;
using WorkTogetherly.Shared.Services;
using WorkTogetherly.Shared.Settings;

namespace WorkTogetherly;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // For Debug
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        //Add Mudblazor
        builder.Services.AddMudServices();

        // Blazor auth
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddSingleton<ITokenStorage, SecureTokenStorage>();
        builder.Services.AddScoped<IGeolocationService, MauiGeolocationService>();
        builder.Services.AddSingleton<TokenStorageService>();
        builder.Services.AddScoped<JwtAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<JwtAuthenticationStateProvider>());
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthService>());
        builder.Services.AddScoped<AppInitializer>();
        builder.Services.AddScoped<IAppInitializer>(sp => sp.GetRequiredService<AppInitializer>());
        builder.Services.AddTransient<AuthTokenHandler>();

        // Business services
        builder.Services.AddScoped<PageTitleService>();
        builder.Services.AddScoped<WorkspaceService>();
        builder.Services.AddScoped<SlotService>();
        builder.Services.AddScoped<AmenityService>();
        builder.Services.AddScoped<BookingService>();
        builder.Services.AddScoped<ReviewService>();
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<MessageService>();
        builder.Services.AddScoped<NotificationHubService>();

        // HTTP clients
        const string apiBaseUrl = "https://api.worktogetherly.be";
        builder.Services.AddSingleton(new ApiSettings(apiBaseUrl));

        // Public client without auth
        builder.Services.AddHttpClient("Public", client =>
            client.BaseAddress = new Uri(apiBaseUrl + "/"));

        // Authenticated client with token handler
        builder.Services.AddHttpClient("Auth", client =>
            client.BaseAddress = new Uri(apiBaseUrl + "/"))
            .AddHttpMessageHandler<AuthTokenHandler>();

        // Geocoding client for external API
        builder.Services.AddHttpClient("Geocoding", client =>
            client.BaseAddress = new Uri("https://photon.komoot.io/"));

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}