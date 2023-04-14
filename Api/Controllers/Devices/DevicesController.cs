using Contracts.Devices;
using DatabaseServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using Models.Phones;

namespace Api.Controllers.Devices
{
    /// <summary>
    /// Device setup steps:
    /// 1). User powers on device
    /// 2). Device opens up web portal for user to input wifi to
    /// 3). User presses button on device to enter pairing mode
    /// 4). Device then sends request to api for pairing with its device id
    /// 5). Api then registers the device with IoT Hub, generates certificate,
    /// and assigns that device to the user. Returns a message to the device
    /// with the certificate and IoT key.
    /// 6). Device then begins job, periodically sending http post requests to api
    /// 7). User can use online portal, or mobile device to manually request data
    /// from the device, utilizing IoT to device MQTT messaging. 
    /// </summary>
    [ApiController]
    [Route("[controller]")]

    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDeviceRegistryService _deviceRegistryService;

        public DevicesController(AppDbContext dbContext, UserManager<ApplicationUser> userManager, IDeviceRegistryService deviceRegistryService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _deviceRegistryService = deviceRegistryService;
        }
        [HttpPost]
        [Route("ping")]

        public async Task<IActionResult> Ping([FromBody] PingDTO pingDto)
        {
            string msg = pingDto.Message;

            Console.WriteLine(msg);
            return Ok(msg);
        }

        /// <summary>
        /// Device is plugged in, if device does NOT have a device id and x509 certificate, then ping api for a new one. Will both
        /// register the device in IoT and in db (without owner)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("RegistrationRequest")]
        public async Task<IActionResult> RegistrationRequest([FromBody] DeviceRegistryRequest registryRequest)
        {
            try
            {
                if (_dbContext.Devices.FirstOrDefaultAsync(x => x.HardwareId == registryRequest.DeviceHardWareId) != null)
                    throw new Exception($"Device with hardware id {registryRequest.DeviceHardWareId} already exists in database");

                Device device = await _deviceRegistryService.CreateAndRegisterDevice(registryRequest);
                string assignedId = Guid.NewGuid().ToString();
                DeviceRegistryResponse response = new DeviceRegistryResponse() {
                    AssignedId = assignedId,
                    X509Thumbprint = device.Authentication.X509Thumbprint.PrimaryThumbprint
                };
                
                Models.Devices.Device modelDevice = new Models.Devices.Device()
                {
                    Id = assignedId,
                    HardwareId = registryRequest.DeviceHardWareId,
                    GenerationId = 1,
                    ETag = Guid.NewGuid().ToString().Substring(0, 8),
                    ConnectionState = DeviceConnectionState.Disconnected,
                    Status = DeviceStatus.Disabled,
                    CloudToDeviceMessageCount = 1

                };
          

                await _dbContext.Devices.AddAsync(modelDevice);
                await _dbContext.SaveChangesAsync();
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Route("GetDevices")]
        public async Task<List<Models.Devices.Device>> GetDevices()
        {
            try
            {
                var devices = await _dbContext.Devices.ToListAsync();
                return devices;
            }
            catch(Exception e)
            {
                return new List<Models.Devices.Device>();
            }
        }

        [HttpGet]
        [Route("GetData")]
        public async Task<IActionResult> GetData()
        {
            return Ok(new { Message = "test" });
        }


    }
}
