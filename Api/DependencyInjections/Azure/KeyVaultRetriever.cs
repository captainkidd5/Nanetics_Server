using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Api.DependencyInjections.Azure
{
    public class KeyVaultRetriever : IKeyVaultRetriever
    {
        private readonly IConfiguration _configuration;
        private readonly SecretClient _secretClient;
        public KeyVaultRetriever(IConfiguration configuration)
        {
            _configuration = configuration;
            string keyVaultName = _configuration.GetSection("Azure").GetSection("KeyVaultName").Value;
            string kvUri = "https://" + keyVaultName + ".vault.azure.net";

            _secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
        }
        public KeyVaultSecret RetrieveKey(string val)
        {
            try
            {
               
                return _secretClient.GetSecret(val).Value;
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error {e}");
            }
           return null; 

        }
    }
}
