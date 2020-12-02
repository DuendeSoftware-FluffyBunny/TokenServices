using System.Threading.Tasks;
namespace FluffyBunny4.Services
{
    public interface IKeyVaultSecretProvider
    {
        public Task<string> GetSecretAsync(string keyVaultName, string secretName);
    }
}
