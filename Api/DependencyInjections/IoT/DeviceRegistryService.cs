using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Api.DependencyInjections.Azure;
using Contracts.Devices;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Models.Authentication;


namespace Api.DependencyInjections.IoT
{
    /// <summary>
    /// <see cref="https://github.com/Azure/azure-iot-sdk-csharp/blob/main/iothub/service/samples/how%20to%20guides/RegistryManagerSample/RegistryManagerSample.cs"/>
    /// </summary>
    public class DeviceRegistry : IDeviceRegistryService
    {
        private readonly IKeyVaultRetriever _keyVaultRetriever;

        public DeviceRegistry(IKeyVaultRetriever keyVaultRetriever)
        {
            _keyVaultRetriever = keyVaultRetriever;
        }

  


        public async Task<Device> CreateAndRegisterDevice(DeviceRegistryRequest registryRequest)
        {
            using RegistryManager registryManager = RegistryManager
              .CreateFromConnectionString(_keyVaultRetriever.RetrieveKey("IoTHubConnectionString").Value);

            Device device;
            string deviceId = GenerateDeviceId(registryRequest.DeviceHardWareId);
            X509Certificate2 cert = CreateSelfSignCert(registryRequest.DeviceHardWareId);
            try
            {
                device = new Device(deviceId)
                {
                    Authentication = new AuthenticationMechanism
                    {
                        Type = AuthenticationType.SelfSigned,
                        X509Thumbprint = new X509Thumbprint
                        {
                            PrimaryThumbprint = cert.Thumbprint
                        }
                    }
                };

                device = await registryManager.AddDeviceAsync(device);
                return device;
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
                device.Authentication = new AuthenticationMechanism
                {
                    Type = AuthenticationType.SelfSigned,
                    X509Thumbprint = new X509Thumbprint
                    {
                        PrimaryThumbprint = cert.Thumbprint
                    }
                };
              
                await registryManager.UpdateDeviceAsync(device);
                return device;
            }

            string deviceConnectionString = $"HostName=NaneticsIoTHub.azure-devices.net;DeviceId={device.Id};X509Certificate={Convert.ToBase64String(cert.Export(X509ContentType.Cert))}";

            // Use the devi
        }

        //public string GetDeviceConnectionString(string deviceId)
        //{

        //}

        private string GenerateDeviceId(string deviceHardwarId)
        {
            string deviceId = deviceHardwarId + "_" + Guid.NewGuid().ToString().Substring(0, 8);
            return deviceId;
        }

        private X509Certificate2 CreateSelfSignCert(string deviceName)
        {
            string subjectName = deviceName; // e.g. "CN=MyDevice"
            DateTimeOffset validFrom = DateTimeOffset.UtcNow.AddDays(-1);
            DateTimeOffset validTo = DateTimeOffset.UtcNow.AddDays(365);
            X509Certificate2 deviceCertificate = new X509Certificate2();
            using (RSA rsa = RSA.Create(2048))
            {
                var certRequest = new CertificateRequest($"CN={deviceName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                certRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
                certRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
                certRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.2") }, false));
                deviceCertificate = certRequest.CreateSelfSigned(validFrom, validTo);
            }
            return deviceCertificate;
        }
    }
}
