using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Template.Application.Common.Settings;

namespace Template.Infrastructure.Configuration
{
    /// <summary>
    /// Database configuration and setup
    /// </summary>
    public static class DatabaseConfiguration
    {
        public static IServiceCollection AddDatabaseConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

            services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return client.GetDatabase(settings.DatabaseName);
            });

            return services;
        }
    }
}
