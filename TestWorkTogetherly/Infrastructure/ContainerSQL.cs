using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;
using WorkTogetherly.Infrastructure.Persistence;

namespace TestWorkTogetherly.Infrastructure
{
    public class ContainerSQL : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
            .WithDatabase("worktogetherly_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        public required string ConnectionStringMaster;
        public required AppDbContext _context;

        public async Task InitializeAsync()
        {
            await _container.StartAsync();
            ConnectionStringMaster = _container.GetConnectionString();
            await CreateDatabase();
            await SeedDataInDatabase();
        }

        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }

        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(ConnectionStringMaster)
                .Options;
            return new AppDbContext(options, Substitute.For<IPublisher>());
        }

        private async Task CreateDatabase()
        {
            _context = CreateDbContext();
            await _context.Database.MigrateAsync();
            Console.WriteLine("Database created");
        }

        public async Task SeedDataInDatabase()
        {
            await _context.SaveChangesAsync();
        }
    }
}
