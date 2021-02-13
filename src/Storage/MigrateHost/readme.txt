Add-Migration initial -c MainEntityCoreContext				-StartupProject MigrateHost  -Project Migrations.SqlServer -o Migrations/Main
Add-Migration initial -c TenantAwareConfigurationDbContext	-StartupProject MigrateHost  -Project Migrations.SqlServer -o Migrations/Tenant
