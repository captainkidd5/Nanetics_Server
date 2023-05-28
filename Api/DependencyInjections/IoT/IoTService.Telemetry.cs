using Contracts.Devices.IoT;
using Contracts.Devices.IoT.Telemetry;

namespace Api.DependencyInjections.IoT
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/rest/api/iotcentral/2022-10-31-previewdataplane/devices/get-telemetry-value?tabs=HTTP#devicetelemetry
    /// </summary>
    public partial class IoTService : IIotService
    {
        public async Task<IoTTelemetry> GetTelemetryForDevice(string deviceId, string telemetryName)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"devices/{deviceId}/telemetry/{telemetryName}?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            AddApiAuthorization(msg);

            HttpResponseMessage result = await client.SendAsync(msg);

            if (!result.IsSuccessStatusCode)
            {
                ErrorDetails? response = await result.Content.ReadFromJsonAsync<ErrorDetails>();
                string errorMsg = response.message;
                return null;
            }

            IoTTelemetry iotTelemetry = await result.Content.ReadFromJsonAsync<IoTTelemetry>();
            return iotTelemetry;

        }
    }
}
