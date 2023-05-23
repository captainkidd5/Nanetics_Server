using Contracts.Devices.IoT;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Api.DependencyInjections.IoT
{
    public partial class IoTService : IIotService
    {

        /// <summary>
        /// https://learn.microsoft.com/en-us/rest/api/iotcentral/2022-07-31dataplane/enrollment-groups/create-x509?tabs=HTTP
        /// </summary>
        /// <param name="enrollmentGroupId"></param>
        /// <param name="certificateName"></param>
        /// <returns></returns>
        public async Task<X509Certificate2> CreateEnrollmentGroupX509(string enrollmentGroupId, string certificateName)
        {
            HttpClient client = _httpClientFactory.CreateClient();


            CertificateEntry certificateEntry = new CertificateEntry()
            {

            };
            string requestUrl = _iotString + $"enrollmentGroups/{enrollmentGroupId}/certificates/{entry}?api-version={apiVersion}";

            CreateEnrollmentGroupX509Request createEnrollmentGroupX509Request = new CreateEnrollmentGroupX509Request()
            {

            };

              var json = JsonConvert.SerializeObject(createEnrollmentGroupX509Request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(requestUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseContent);

            string certificateString = result.certificate;
            byte[] certificateBytes = Convert.FromBase64String(certificateString);

            return new X509Certificate2(certificateBytes);
        }

        public async Task<bool> VerifyCertificateAsync(X509Certificate2 certificate)
        {
            HttpClient client = _httpClientFactory.CreateClient();

            string requestUrl = _iotString + $"devices/verify?api-version={apiVersion}";

            var payload = new
            {
                certificate = Convert.ToBase64String(certificate.Export(X509ContentType.Cert))
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(requestUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseContent);

            bool isVerified = result.verify;

            return isVerified;
        }

        private X509Certificate2 CreateSelfSignCert(ulong hardwareId)
        {
            string subjectName = hardwareId.ToString(); // e.g. "CN=MyDevice"
            DateTimeOffset validFrom = DateTimeOffset.UtcNow.AddDays(-1);
            DateTimeOffset validTo = DateTimeOffset.UtcNow.AddDays(365);
            X509Certificate2 deviceCertificate = new X509Certificate2();
            using (RSA rsa = RSA.Create(2048))
            {
                var certRequest = new CertificateRequest($"CN={hardwareId}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                certRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
                certRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
                certRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.2") }, false));
                deviceCertificate = certRequest.CreateSelfSigned(validFrom, validTo);
            }

          
            return deviceCertificate;
        }
    }
}
