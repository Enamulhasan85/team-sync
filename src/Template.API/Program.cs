using MongoDB.Driver;
using Template.API.Extensions;
using Template.Application;
using Template.Infrastructure;

namespace Template.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddWebApiServices(builder.Configuration);

            var app = builder.Build();


            // Seed MongoDB database with default data (no migrations needed for MongoDB)
            await app.SeedDatabaseAsync();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseResponseCompression();

            app.UseExceptionHandler("/error");

            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
