﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Identity;

namespace Template.Infrastructure.Data.Contexts
{
    public class IdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        // Apply custom configurations
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Only apply Identity-specific configurations here
            // Don't apply all configurations from assembly
        }
    }
}
