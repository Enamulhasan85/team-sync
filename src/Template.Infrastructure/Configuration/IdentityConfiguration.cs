using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Template.Domain.Identity;

namespace Template.Infrastructure.Configuration
{
    /// <summary>
    /// Identity configuration and setup for MongoDB
    /// </summary>
    public static class IdentityConfiguration
    {
        public static IServiceCollection AddIdentityConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var appSettings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();

            // Configure MongoDB Identity
            var mongoDbIdentityConfiguration = new MongoDbIdentityConfiguration
            {
                MongoDbSettings = appSettings,
                IdentityOptionsAction = options =>
                {
                    // Password settings
                    options.Password.RequiredLength = 8;

                    // User settings
                    options.User.RequireUniqueEmail = true;

                    // Sign-in settings
                    options.SignIn.RequireConfirmedEmail = true;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                    options.SignIn.RequireConfirmedAccount = false;
                }
            };

            services.ConfigureMongoDbIdentity<ApplicationUser, MongoIdentityRole<ObjectId>, ObjectId>(mongoDbIdentityConfiguration)
                .AddDefaultTokenProviders()
                .AddSignInManager<SignInManager<ApplicationUser>>();

            return services;
        }
    }
}
