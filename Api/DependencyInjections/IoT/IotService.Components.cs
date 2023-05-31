using Contracts.Devices.IoT;
using Microsoft.Azure.Devices.Provisioning.Client;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;


namespace Api.DependencyInjections.IoT
{
    public partial class IoTService : IIotService
    {
        public async Task<IotCollection> GetIoTDeviceComponents(string deviceId)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"devices/{deviceId}/components?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            AddApiAuthorization(msg);

            HttpResponseMessage result = await client.SendAsync(msg);

            if (!result.IsSuccessStatusCode)
            {
                ErrorDetails? response = await result.Content.ReadFromJsonAsync<ErrorDetails>();
                string errorMsg = response.message;
                return null;
            }

            IotCollection d = await result.Content.ReadFromJsonAsync<IotCollection>();

            return d;

        }

        public async Task<IoTTemplateProperties> GetIoTDeviceProperties(string deviceId)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"devices/{deviceId}/properties?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            AddApiAuthorization(msg);

            HttpResponseMessage result = await client.SendAsync(msg);

            if (!result.IsSuccessStatusCode)
            {
                ErrorDetails? response = await result.Content.ReadFromJsonAsync<ErrorDetails>();
                string errorMsg = response.message;
                return null;
            }

            IoTTemplateProperties d = await result.Content.ReadFromJsonAsync<IoTTemplateProperties>();

            return d;

        }
        public async Task<object> GetIoTDeviceComponentProperties(string deviceId, string componentName)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"devices/{deviceId}/properties?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            AddApiAuthorization(msg);

            HttpResponseMessage result = await client.SendAsync(msg);

            if (!result.IsSuccessStatusCode)
            {
                ErrorDetails? response = await result.Content.ReadFromJsonAsync<ErrorDetails>();
                string errorMsg = response.message;
                return null;
            }
            // dynamic a = await result.Content.ReadFromJsonAsync<dynamic>();
            object d = await result.Content.ReadFromJsonAsync<object>();

            return d;

        }

        /// <summary>
        /// Non functional
        /// https://learn.microsoft.com/en-us/azure/iot-central/core/howto-control-devices-with-rest-api
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="soilSensorProperties"></param>
        /// <returns></returns>
        public async Task<IoTTemplateProperties> UpdateIoTComponentProperties(string deviceId, IoTTemplateProperties soilSensorProperties)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"devices/{deviceId}/components/deviceInformation/properties?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Patch, endPoint);
            AddApiAuthorization(msg);

            IoTTemplateModuleUpdateProperties soilSensorModuleUpdateProperties = new IoTTemplateModuleUpdateProperties();
            soilSensorModuleUpdateProperties.body = soilSensorProperties.deviceInformation;
            string json = JsonConvert.SerializeObject(soilSensorModuleUpdateProperties);

            msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage result = await client.SendAsync(msg);

            if (!result.IsSuccessStatusCode)
            {
                ErrorDetails? response = await result.Content.ReadFromJsonAsync<ErrorDetails>();
                string errorMsg = response.message;
                return null;
            }
            IoTTemplateProperties d = await result.Content.ReadFromJsonAsync<IoTTemplateProperties>();

            return d;

        }
    }
}