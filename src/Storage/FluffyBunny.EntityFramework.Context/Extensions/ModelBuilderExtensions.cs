using Duende.IdentityServer.EntityFramework.Options;
using FluffyBunny.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluffyBunny.EntityFramework.Context.Extensions
{
    /// <summary>
    /// Extension methods to define the database schema for the configuration and operational data stores.
    /// </summary>
    public static class ModelBuilderExtensions
    {
        private static EntityTypeBuilder<TEntity> ToTable<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            TableConfiguration configuration)
            where TEntity : class
        {
            return string.IsNullOrWhiteSpace(configuration.Schema)
                ? entityTypeBuilder.ToTable(configuration.Name)
                : entityTypeBuilder.ToTable(configuration.Name, configuration.Schema);
        }
        /// <summary>
        /// Configures the ExternalService context.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>

        public static void ConfigureExternalServicesContext(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExternalService>(externalService =>
            {
                externalService.ToTable("ExternalService");
                externalService.HasKey(x => x.Id);
                externalService.Property(x => x.Name).HasMaxLength(200).IsRequired();
                externalService.HasIndex(x => x.Name).IsUnique();

            });

        }

        public static void ConfigureTenantContext(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>(tenant =>
            {
                tenant.ToTable("Tenant");
                tenant.HasKey(x => x.Id);
                tenant.Property(x => x.Name).HasMaxLength(200).IsRequired();
                tenant.HasIndex(x => x.Name).IsUnique();

            });

        }

        
    }
}
