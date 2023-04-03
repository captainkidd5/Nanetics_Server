using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace SilverMenu.DependencyInjections.Azure
{
    public class KeyVaultRetriever : IKeyVaultRetriever
    {
        private readonly IConfiguration _configuration;

        public KeyVaultRetriever(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public KeyVaultSecret RetrieveKey(string val)
        {
            try
            {
                string keyVaultName = _configuration.GetSection("Azure").GetSection("KeyVaultName").Value;
                string kvUri = "https://" + keyVaultName + ".vault.azure.net";

                SecretClient client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
                return client.GetSecret(val).Value;
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error {e}");
            }
           return null; 

        }
    }
}
