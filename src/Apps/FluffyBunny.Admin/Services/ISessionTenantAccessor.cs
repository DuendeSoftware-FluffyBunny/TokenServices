namespace FluffyBunny.Admin.Services
{
    public interface ISessionTenantAccessor
    {
        string TenantId { get; set; }
    }
}
