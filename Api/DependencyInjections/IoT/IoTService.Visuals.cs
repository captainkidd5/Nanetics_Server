using Contracts.Devices.IoT;
using Contracts.Devices.IoT.Visuals;

namespace Api.DependencyInjections.IoT
{
    public partial class IoTService : IIotService
    {
        //public async Task<DashboardCollection> GetIotDashboards(string deviceId)
        //{
        //    HttpClient client = _httpClientFactory.CreateClient();
        //    string endPoint = _iotString + $"devices/{deviceId}/components?api-version={apiVersion}";

        //    HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
        //    AddApiAuthorization(msg);

        //    HttpResponseMessage result = await client.SendAsync(msg);

        //    if (!result.IsSuccessStatusCode)
        //    {
        //        ErrorDetails? response = await result.Content.ReadFromJsonAsync<ErrorDetails>();
        //        string errorMsg = response.message;
        //        return null;
        //    }

        //    IotCollection d = await result.Content.ReadFromJsonAsync<IotCollection>();

        //    return d;

        //}
    }
}
