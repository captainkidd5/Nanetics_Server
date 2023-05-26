using Contracts.Devices.IoT;
using Microsoft.Azure.Devices.Provisioning.Client;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;


namespace Api.DependencyInjections.IoT
{
    public partial class IoTService : IIotService
    {
        public async Task<IotCollection> GetDeviceComponents(string deviceId)
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

        public async Task<SoilSensorProperties> GetDeviceProperties(string deviceId)
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

            SoilSensorProperties d = await result.Content.ReadFromJsonAsync<SoilSensorProperties>();

            return d;

        }
    }
}