using Contracts.Devices.IoT;
using Newtonsoft.Json;
using System.Text;

namespace Api.DependencyInjections.IoT
{
    public partial class IoTService : IIotService
    {
        public async Task<IoTCommandDTO> SendIoTCommand(string deviceId, string commandName, string json = null)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                string endPoint = _iotString + $"devices/{deviceId}/commands/{commandName}?api-version={apiVersion}";

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, endPoint);
                AddApiAuthorization(msg);
                //  template = $"dtmi:modelDefinition:naneticshub:soil_sensorv2;1",

         

                if(!string.IsNullOrEmpty(json))
                {
                    msg.Content = new StringContent(json, Encoding.UTF8, "application/json");

                }

                HttpResponseMessage result = await client.SendAsync(msg);
                if (!result.IsSuccessStatusCode)
                {
                    IoTResponseError? response = await result.Content.ReadFromJsonAsync<IoTResponseError>();

                    string errorMsg = response.error.message;
                    return null;

                }

                IoTCommandDTO iotCommandResponse = await result.Content.ReadFromJsonAsync<IoTCommandDTO>();

                return iotCommandResponse;
            }
            catch (Exception e)
            {
                return null;
            }


        }
    }
}
