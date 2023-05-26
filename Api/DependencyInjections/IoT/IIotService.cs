using Contracts.Devices.IoT;
using Microsoft.Azure.Devices;

namespace Api.DependencyInjections.IoT
{
    public interface IIotService
    {
        public  Task<HttpResponseMessage> CreateApiToken();

        public  Task<IoTDeviceDTO> AddDevice(string deviceId);

        public  Task<HttpResponseMessage> GetDeviceCredentials(string deviceId);

        public  Task<HttpResponseMessage> GetDevice(string deviceId);

        public Task<HttpResponseMessage> QueryDevice(string deviceId);
        public Task<bool> DeleteDevice(string deviceId);

        public Task<bool> UpdateDevice(string deviceId, UpdateIoTDeviceRequest updateRequest);

        public Task<HttpResponseMessage> GetTemplates();

        public Task<IoTDeviceCollectionDTO> GetAllDevices();

        public Task<IotCollection> GetDeviceComponents(string deviceId);

        public Task<SoilSensorProperties> GetDeviceProperties(string deviceId);
    }
}
