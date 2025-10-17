using System.Reflection;
using System.Text;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Template.API.Common.Extensions;
using Template.Application.Common.Settings;

namespace Template.API.Extensions
{
    /// <summary>
    /// API layer service collection extensions
    /// Contains only configurations specific to the Web API layer
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebApiServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddApiAuthentication(configuration);
            services.AddAuthorization();

            services.AddRateLimiting(configuration);

            services.AddResponseCompression();

            services.AddApiControllers();

            services.AddCustomApiVersioning();

            services.AddApiValidation();

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddEndpointsApiExplorer();

            services.AddSwaggerServices();

            services.AddApiServices();

            services.AddApiCors(configuration);

            return services;
        }


        private static IServiceCollection AddApiAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings configuration is required");

            var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Remove default 5-minute tolerance
                };
            });

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            return services;
        }


        private static IServiceCollection AddApiControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<Template.API.Common.Filters.ExceptionFilter>();
                options.Filters.Add<Template.API.Common.Filters.GlobalValidationFilter>();
                options.Filters.Add(new Template.API.Common.Filters.GlobalPaginationValidationFilter(maxPageSize: 100));
            });

            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            return services;
        }


        private static IServiceCollection AddApiValidation(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();

            return services;
        }


        private static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            // Register API-specific services here if needed

            return services;
        }

        private static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();
            if (allowedOrigins != null && allowedOrigins.Length > 0)
            {
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod());
                });
            }
            else
            {
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod());
                });
            }

            return services;
        }
    }
}
