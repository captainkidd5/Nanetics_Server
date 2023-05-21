using Api.DependencyInjections.Azure;
using Contracts.Devices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System.Text;

namespace Api.DependencyInjections.IoT
{
    public class IoTService : IIotService
    {
        private const string _iotString = "https://naneticshub.azureiotcentral.com/api/";
        private const byte apiVersion = 1;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IKeyVaultRetriever _keyVaultRetriever;

        public IoTService(IHttpClientFactory httpClientFactory, IKeyVaultRetriever keyVaultRetriever)
        {
            _httpClientFactory = httpClientFactory;
            _keyVaultRetriever = keyVaultRetriever;
        }
        public async Task<HttpResponseMessage> CreateApiToken()
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string tokenId = Guid.NewGuid().ToString();
            string endPoint = _iotString + $"api/apiTokens/{tokenId}?api-version=2022-10-31-preview";
            // Create the role assignment array
            RoleAssignmentDTO[] roleAssignments = new RoleAssignmentDTO[]
            {
        new RoleAssignmentDTO { Role = "Admin" }
            };

            string json = JsonConvert.SerializeObject(roleAssignments);

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Put, endPoint);
            msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
            msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_keyVaultRetriever.RetrieveKey("IoTCentralApiToken").Value);
            HttpResponseMessage result = await client.SendAsync(msg);
            if(result.IsSuccessStatusCode)
            {
                IoTAPITokenResponseDTO? response = await result.Content.ReadFromJsonAsync<IoTAPITokenResponseDTO>();
            }
            if (!result.IsSuccessStatusCode)
            {
                ErrorDetails? response = await result.Content.ReadFromJsonAsync<ErrorDetails>();
                string errorMsg = response.Message;
            }
            return result;

        }
        //public async Task<HttpResponseMessage> CreateOperatorToken()
        //{
        //    HttpClient client = _httpClientFactory.CreateClient();
        //    string endPoint = _iotString + $"apiTokens/operator-token?api-version={apiVersion}";

        //    HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Put, endPoint);
        //    HttpResponseMessage result = await client.SendAsync(msg);
        //    return result;

        //}

        //public async Task<HttpResponseMessage> CreateAdminToken()
        //{
        //    HttpClient client = _httpClientFactory.CreateClient();
        //    string endPoint = _iotString + $"apiTokens/admin-token?api-version={apiVersion}";

        //    HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Put, endPoint);
        //    HttpResponseMessage result = await client.SendAsync(msg);
        //    return result;

        //}
        private void AddApiAuthorization(HttpRequestMessage msg)
        {
            try
            {
                string value = _keyVaultRetriever.RetrieveKey("IoTCentralApiToken").Value;
                value = value.Split("SharedAccessSignature ")[1];
                msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", value);

            }
            catch (Exception ex)
            {
                Console.WriteLine("test");
            }
            

        }
        public async Task<Device> AddDevice(string deviceHardWareId)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                string endPoint = _iotString + $"devices/{deviceHardWareId}?api-version=2022-10-31-preview";

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Put, endPoint);
                AddApiAuthorization(msg);
                CreateDeviceRequestDTO createDeviceRequestDTO = new CreateDeviceRequestDTO()
                {
                    DisplayName = deviceHardWareId,
                    Enabled = false,
                    ETag = deviceHardWareId,
                    Simulated = false,
                    Template = string.Empty,


                };
                string json = JsonConvert.SerializeObject(createDeviceRequestDTO);

                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage result = await client.SendAsync(msg);
                if (!result.IsSuccessStatusCode)
                {
                    ErrorDetails? response = await result.Content.ReadFromJsonAsync<ErrorDetails>();
                    string errorMsg = response.Message;
                }
                //IoTDeviceDTO iotDevice = await result.Content.ReadFromJsonAsync<IoTDeviceDTO>();
                string responseContent = await result.Content.ReadAsStringAsync();

                object iotDevice = await result.Content.ReadFromJsonAsync<object>();

                Device d = new Device();
                return d;
            }
            catch(Exception e)
            {
                int num = 1;
                return null;
            }
          

        }

        public async Task<HttpResponseMessage> GetDeviceCredentials(string deviceId)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"devices/{deviceId}/credentials?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            AddApiAuthorization(msg);

            HttpResponseMessage result = await client.SendAsync(msg);
            return result;

        }

        public async Task<HttpResponseMessage> GetDevice(string deviceId)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"devices/{deviceId}?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            AddApiAuthorization(msg);

            HttpResponseMessage result = await client.SendAsync(msg);
            return result;

        }
        public async Task<bool> DeleteDevice(string deviceId)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                string endPoint = _iotString + $"devices/{deviceId}?api-version={apiVersion}";

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Delete, endPoint);
                AddApiAuthorization(msg);

                HttpResponseMessage result = await client.SendAsync(msg);
                return result.IsSuccessStatusCode;
            }
            catch(Exception ex)
            {
                return false;
            }
           

        }
        public async Task<HttpResponseMessage> QueryDevice(string deviceId)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"query?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            HttpResponseMessage result = await client.SendAsync(msg);
            return result;

        }
    }
}
