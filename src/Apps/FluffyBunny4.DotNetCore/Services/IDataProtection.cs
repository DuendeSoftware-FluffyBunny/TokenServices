namespace FluffyBunny4.DotNetCore.Services
{
    public interface IDataProtection
    {
        string ProtectString(string unprotectedText);
        string UnprotectString(string protectedText);
    }
}
