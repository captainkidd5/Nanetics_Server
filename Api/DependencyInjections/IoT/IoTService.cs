using Contracts.Devices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Api.DependencyInjections.IoT
{
    public class IoTService
    {
        private const string _iotString = "https://naneticshub.azureiotcentral.com/api";
        private const byte apiVersion = 1;

        private readonly IHttpClientFactory _httpClientFactory;

        public IoTService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

            HttpResponseMessage result = await client.SendAsync(msg);
            if(result.IsSuccessStatusCode)
            {
                IoTAPITokenResponseDTO? response = await result.Content.ReadFromJsonAsync<IoTAPITokenResponseDTO>();
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

        public async Task<HttpResponseMessage> AddDevice(string deviceId)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"devices/{deviceId}?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Put, endPoint);

            CreateDeviceRequestDTO createDeviceRequestDTO = new CreateDeviceRequestDTO()
            {

            };
            string json = JsonConvert.SerializeObject(roleAssignments);

            msg.Content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage result = await client.SendAsync(msg);
            return result;

        }

        public async Task<HttpResponseMessage> GetDeviceCredentials(string deviceId)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"devices/{deviceId}/credentials?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            HttpResponseMessage result = await client.SendAsync(msg);
            return result;

        }

        public async Task<HttpResponseMessage> GetDevice(string deviceId)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"devices/{deviceId}?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            HttpResponseMessage result = await client.SendAsync(msg);
            return result;

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
