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

        public static void ConfigureAllowedArbitraryIssuerContext(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AllowedArbitraryIssuer>(entity =>
            {
                entity.ToTable("AllowedArbitraryIssuer");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Issuer).HasMaxLength(2000).IsRequired();
                entity.HasIndex(x => x.Issuer).IsUnique();

            });

        }
        public static void ConfigureAllowedRevokeTokenTypeHintContext(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AllowedRevokeTokenTypeHint>(entity =>
            {
                entity.ToTable("AllowedRevokeTokenTypeHint");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TokenTypeHint).HasMaxLength(64).IsRequired();
                entity.HasIndex(x => x.TokenTypeHint).IsUnique();

            });

        }

        public static void ConfigureAllowedTokenExchangeExternalServiceContext(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AllowedTokenExchangeExternalService>(entity =>
            {
                entity.ToTable("AllowedTokenExchangeExternalService");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ExternalService).HasMaxLength(64).IsRequired();
                entity.HasIndex(x => x.ExternalService).IsUnique();

            });

        }

        
        public static void ConfigureExternalServicesContext(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExternalService>(entity =>
            {
                entity.ToTable("ExternalService");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.HasIndex(x => x.Name).IsUnique();

            });

        }
        public static void ConfigureTenantContext(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.ToTable("Tenant");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.HasIndex(x => x.Name).IsUnique();

            });

        }

        
    }
}
