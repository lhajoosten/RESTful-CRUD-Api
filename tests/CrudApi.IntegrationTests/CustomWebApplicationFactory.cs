using CrudApi.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CrudApi.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a database context using an in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

            // Ensure the database is created
            db.Database.EnsureCreated();

            try
            {
                // Seed the database with test data if needed
                SeedTestData(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the test database");
            }
        });

        builder.UseEnvironment("Testing");
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Add any test data seeding here if needed
        // This is optional and can be customized based on test requirements
        
        // Example: Add test categories
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new CrudApi.Core.Entities.Category
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111111"),
                    Name = "Electronics",
                    Slug = "electronics",
                    Description = "Electronic products",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new CrudApi.Core.Entities.Category
                {
                    Id = new Guid("22222222-2222-2222-2222-222222222222"),
                    Name = "Books",
                    Slug = "books",
                    Description = "Books and literature",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }

        // Example: Add test products
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new CrudApi.Core.Entities.Product
                {
                    Id = new Guid("33333333-3333-3333-3333-333333333333"),
                    Name = "iPhone 15",
                    Description = "Latest Apple smartphone",
                    Price = 999.99m,
                    CategoryId = new Guid("11111111-1111-1111-1111-111111111111"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new CrudApi.Core.Entities.Product
                {
                    Id = new Guid("44444444-4444-4444-4444-444444444444"),
                    Name = "The Great Gatsby",
                    Description = "Classic American novel",
                    Price = 15.99m,
                    CategoryId = new Guid("22222222-2222-2222-2222-222222222222"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }

        context.SaveChanges();
    }
}
