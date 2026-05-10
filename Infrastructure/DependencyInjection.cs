using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Interfaces;
using WorkTogetherly.Infrastructure.Persistence;
using WorkTogetherly.Infrastructure.Repositories;
using WorkTogetherly.Infrastructure.Services.Auth;
using WorkTogetherly.Infrastructure.Settings;
using Resend;
using Microsoft.AspNetCore.Identity;

namespace WorkTogetherly.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrWhiteSpace(databaseUrl))
            {
                var uri = new Uri(databaseUrl);
                var userInfo = uri.UserInfo.Split(':');
                connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]}";
            }

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString));
            }
            else
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("WorkTogetherlyDb"));
            }
            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!))
                };
                // Allows browser requests to send the JWT via HttpOnly cookie
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        // SignalR WebSocket cannot send headers — read the token from the query string
                        var tokenFromQuery = ctx.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(tokenFromQuery) &&
                            ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            ctx.Token = tokenFromQuery;
                        }
                        else if (string.IsNullOrEmpty(ctx.Token))
                        {
                            ctx.Token = ctx.Request.Cookies["access_token"];
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Email (Resend)
            services.Configure<ResendSettings>(configuration.GetSection("Resend"));
            services.Configure<Application.Settings.FrontendSettings>(configuration.GetSection("Frontend"));
            services.AddOptions();
            services.AddHttpClient<ResendClient>();
            services.Configure<ResendClientOptions>(o =>
            {
                o.ApiToken = configuration["Resend:ApiKey"] ?? string.Empty;
            });
            services.AddTransient<IResend, ResendClient>();
            services.AddScoped<IEmailService, ResendEmailService>();

            // Services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddSingleton<IClock, SystemClock>();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
            services.AddScoped<ISlotRepository, SlotRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IMaterialRepository, MaterialRepository>();
            services.AddScoped<IRuleRepository, RuleRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();

            services.AddScoped<DatabaseSeeder>();

            return services;

        }
    }
}
