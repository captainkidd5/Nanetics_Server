using System.Threading.Tasks;
using Contracts.Devices;
using Microsoft.Azure.Devices;
using Models.Authentication;

public interface IDeviceRegistryService
{
    Task<Device> CreateAndRegisterDevice(DeviceRegistryRequest registryRequest);
    Task<bool> UnregisterDevice(string deviceId);
}
