using Api.DependencyInjections.Azure;
using Microsoft.Azure.Devices;
using Models.Authentication;

namespace Api.DependencyInjections.IoT
{
    /// <summary>
    /// <see cref="https://github.com/Azure/azure-iot-sdk-csharp/blob/main/iothub/service/samples/how%20to%20guides/RegistryManagerSample/RegistryManagerSample.cs"/>
    /// </summary>
    public class DeviceRegistry
    {
        private readonly IKeyVaultRetriever _keyVaultRetriever;

        public DeviceRegistry(IKeyVaultRetriever keyVaultRetriever)
        {
            _keyVaultRetriever = keyVaultRetriever;
        }

        public void RegisterDevice(ApplicationUser user)
        {
            using RegistryManager registryManager = RegistryManager
               .CreateFromConnectionString(_keyVaultRetriever.RetrieveKey("IoTHubConnectionString").Value);
        }


        private async Task AddDeviceWithSelfSignedCertificateAsync(RegistryManager registryManager, ApplicationUser user)
        {
            Console.WriteLine("\n=== Creating a device using self-signed certificate authentication ===\n");

            string selfSignedCertDeviceId = GenerateDeviceId(user);

            var device = new Device(selfSignedCertDeviceId)
            {
                Authentication = new AuthenticationMechanism
                {
                    Type = AuthenticationType.SelfSigned,
                    X509Thumbprint = new X509Thumbprint
                    {
                        PrimaryThumbprint = _parameters.PrimaryThumbprint,
                        SecondaryThumbprint = _parameters.SecondaryThumbprint,
                    },
                },
            };

            await registryManager.AddDeviceAsync(device);
            Console.WriteLine($"Added device {selfSignedCertDeviceId} with self-signed certificate auth. ");
        }

        private string GenerateDeviceId(ApplicationUser user)
        {
            string deviceId = user.Id + "_" + Guid.NewGuid().ToString().Substring(0, 8);
            return deviceId;
        }
    }
}
