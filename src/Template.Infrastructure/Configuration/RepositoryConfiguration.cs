using Microsoft.Extensions.DependencyInjection;
using Template.Application.Common.Interfaces;
using Template.Infrastructure.Data;

namespace Template.Infrastructure.Configuration
{
    /// <summary>
    /// Repository and Unit of Work configuration
    /// </summary>
    public static class RepositoryConfiguration
    {
        public static IServiceCollection AddRepositoryConfiguration(
            this IServiceCollection services)
        {
            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
