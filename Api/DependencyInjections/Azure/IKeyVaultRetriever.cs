
using Azure.Security.KeyVault.Secrets;

namespace Api.DependencyInjections.Azure
{
    public interface IKeyVaultRetriever
    {

        public KeyVaultSecret RetrieveKey(string val);
    }
}
