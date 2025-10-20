using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDbGenericRepository;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Settings;
using Template.Infrastructure.Repositories;
using Template.Infrastructure.Services;

namespace Template.Infrastructure.Configuration
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddInfrastructureServicesConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register infrastructure services
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddSingleton<ICacheService, RedisCacheService>();

            services.AddSingleton<IMongoDbContext>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoDbContext(settings.ConnectionString, settings.DatabaseName);
            });

            services.AddScoped(typeof(IRepository<,>), typeof(MongoRepository<,>));

            // RabbitMQ configuration
            services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
            services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

            // Register RabbitMQ consumer as a hosted service
            services.AddHostedService<RabbitMqConsumer>();

            return services;
        }
    }
}
