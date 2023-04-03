
using Azure.Security.KeyVault.Secrets;

namespace SilverMenu.DependencyInjections.Azure
{
    public interface IKeyVaultRetriever
    {

        public KeyVaultSecret RetrieveKey(string val);
    }
}
