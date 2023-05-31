using Contracts.Devices.IoT;
using Contracts.Devices.IoT.Telemetry;
using Microsoft.Azure.Devices;

namespace Api.DependencyInjections.IoT
{
    public interface IIotService
    {
        public  Task<HttpResponseMessage> CreateIoTApiToken();

        public  Task<IoTDeviceDTO> AddIoTDevice(string deviceId);

        public  Task<DeviceCredentials> GetIoTDeviceCredentials(string deviceId);

        public  Task<HttpResponseMessage> GetIoTDevice(string deviceId);

        public Task<HttpResponseMessage> QueryIoTDevice(string deviceId);
        public Task<bool> DeleteIoTDevice(string deviceId);

        public Task<bool> UpdateIoTDevise(string deviceId, UpdateIoTDeviceRequest updateRequest);

        public Task<HttpResponseMessage> GetIoTTemplates();

        public Task<IoTDeviceCollectionDTO> GetAllIoTDevices();

        public Task<IotCollection> GetIoTDeviceComponents(string deviceId);
        public Task<object> GetIoTDeviceComponentProperties(string deviceId, string componentName);
        public Task<IoTTemplateProperties> GetIoTDeviceProperties(string deviceId);

        public Task<IoTTemplateProperties> UpdateIoTComponentProperties(string deviceId, IoTTemplateProperties soilSensorProperties);

        public Task<IoTTelemetry> GetTelemetryForDevice(string deviceId, string telemetryName);

        public Task<Twin> GetTwin(string deviceId);
    }
}
