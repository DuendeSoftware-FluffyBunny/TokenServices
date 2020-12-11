namespace FluffyBunny4.DotNetCore.Services.Defaults
{
    internal class NullDataProtection : IDataProtection
    {
        public string ProtectString(string unprotectedText)
        {
            return unprotectedText;
        }

        public string UnprotectString(string protectedText)
        {
            return protectedText;
        }
    }
}
