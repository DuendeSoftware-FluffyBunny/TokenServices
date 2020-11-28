namespace FluffyBunny4.Services
{
    internal class PasswordGenerator : IPasswordGenerator
    {
        public string GeneratePassword()
        {
            return CodeShare.Library.Passwords
                .PaulSealPasswordGenerator.GeneratePassword(true, true, true, false, false, 32);
        }
    }
}
