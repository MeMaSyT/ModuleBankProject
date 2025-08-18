using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModulebankProject.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace ModulebankProject.Tests.Base;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("ModulebankAccountsDb")
        .WithUsername("postgres")
        .WithPassword("123")
        .Build();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services
                .SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<ModulebankDataContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ModulebankDataContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ModulebankDataContext>();
            dbContext.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS btree_gist;");
            dbContext.Database.Migrate();

            var authDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IAuthenticationService));
            if (authDescriptor != null)
                services.Remove(authDescriptor);

            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "TestScheme", _ => { });
        });
    }

    public Task InitializeAsync()
    {
        return _dbContainer.StartAsync();
    }

    public new Task DisposeAsync()
    {
        return _dbContainer.StopAsync();
    }
}