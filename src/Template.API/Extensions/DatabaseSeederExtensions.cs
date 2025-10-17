using Template.Infrastructure.Data.Seed;

namespace Template.API.Extensions
{
    public static class DatabaseSeederExtensions
    {
        /// <summary>
        /// Seeds the MongoDB database with default data (users, roles, etc.)
        /// MongoDB doesn't require migrations like SQL databases
        /// </summary>
        public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbSeeder>>();

            try
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
                await seeder.SeedAsync();
                logger.LogInformation("MongoDB database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }

            return app;
        }
    }
}
