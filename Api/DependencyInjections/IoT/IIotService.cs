using Contracts.Devices.IoT;
using Microsoft.Azure.Devices;

namespace Api.DependencyInjections.IoT
{
    public interface IIotService
    {
        public  Task<HttpResponseMessage> CreateApiToken();

        public  Task<IoTDeviceDTO> AddDevice(string deviceHardWareId);

        public  Task<HttpResponseMessage> GetDeviceCredentials(string deviceId);

        public  Task<HttpResponseMessage> GetDevice(string deviceId);

        public Task<HttpResponseMessage> QueryDevice(string deviceId);
        public Task<bool> DeleteDevice(string deviceId);
    }
}
