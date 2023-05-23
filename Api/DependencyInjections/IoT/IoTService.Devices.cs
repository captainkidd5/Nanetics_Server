using Contracts.Devices.IoT;
using Microsoft.Azure.Devices.Provisioning.Client;
using Newtonsoft.Json;
using System.Text;

namespace Api.DependencyInjections.IoT
{
    public partial class IoTService : IIotService
    {
        public async Task<Device> AddDevice(string deviceHardWareId)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                string endPoint = _iotString + $"devices/{deviceHardWareId}?api-version={apiVersion}";

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

                    IoTDeviceDTO iotDevice = await result.Content.ReadFromJsonAsync<IoTDeviceDTO>();
                //string responseContent = await result.Content.ReadAsStringAsync();

                //object iotDevice = await result.Content.ReadFromJsonAsync<object>();

                return d;
            }
            catch (Exception e)
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


            string responseContent = await result.Content.ReadAsStringAsync();

            DeviceCredentials credentials = await result.Content.ReadFromJsonAsync<DeviceCredentials>();
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
            catch (Exception ex)
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
