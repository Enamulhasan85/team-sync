using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Template.Application.Common.Behaviors;

namespace Template.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Register MediatR - This scans the assembly for all IRequest handlers
            services.AddMediatR(cfg => 
            {
                cfg.RegisterServicesFromAssembly(assembly);
            });

            // Register AutoMapper
            services.AddAutoMapper(assembly);

            // Register FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // Register Pipeline Behaviors (cross-cutting concerns)
            // These run before/after every MediatR request
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            return services;
        }
    }
}
