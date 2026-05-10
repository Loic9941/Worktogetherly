using WorkTogetherly.Infrastructure;
using WorkTogetherly.Application;
using WorkTogetherly.Infrastructure.Hubs;
using WorkTogetherly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new WorkTogetherly.Presentation.Infrastructure.UtcDateTimeConverter()));
builder.Services.AddSignalR();
// Allow CORS for development: ensures the WebAssembly client can call the API from a different origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", b => b
        .SetIsOriginAllowed(_ => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Infrastructure (DbContext etc.)
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);  

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<WorkTogetherly.Infrastructure.Persistence.DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Serve uploaded workspace photos from wwwroot
app.UseStaticFiles();

// Enable CORS for API endpoints
app.UseCors("DevCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
