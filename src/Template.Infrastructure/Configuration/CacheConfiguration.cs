using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Template.Application.Common.Settings;

namespace Template.Infrastructure.Configuration
{
    public static class CacheConfiguration
    {
        public static IServiceCollection AddCacheConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var memoryCacheSettings = configuration.GetSection("MemoryCacheSettings").Get<InMemoryCacheOptions>() ?? new InMemoryCacheOptions();

            services.Configure<CacheOptions>(configuration.GetSection("CacheSettings"));
            services.Configure<InMemoryCacheOptions>(configuration.GetSection("MemoryCacheSettings"));

            services.AddMemoryCache(options =>
            {
                options.SizeLimit = memoryCacheSettings.MaxCacheSize;
                options.CompactionPercentage = 0.05;
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
            });

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConnection = sp.GetRequiredService<IConfiguration>().GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(redisConnection!);
            });

            return services;
        }
    }
}
