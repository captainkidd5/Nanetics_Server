﻿using Api.DependencyInjections.Authentication;
using AutoMapper;
using Contracts.Authentication.Identity.Create;
using Contracts.Devices;
using DatabaseServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using Models.Devices;
using Models.GroupingStuff;
using Models.Phones;
using Device = Models.Devices.Device;

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
        private readonly IMapper _mapper;
        private readonly IAuthManager _authManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDeviceRegistryService _deviceRegistryService;

        public DevicesController(AppDbContext dbContext,IMapper mapper, IAuthManager authManager, UserManager<ApplicationUser> userManager, IDeviceRegistryService deviceRegistryService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _authManager = authManager;
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
                ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
                if (user == null)
                    return Unauthorized("Invalid refresh token");


                if (_dbContext.Devices.FirstOrDefaultAsync(x => x.HardwareId == registryRequest.DeviceHardWareId) != null)
                    throw new Exception($"Device with hardware id {registryRequest.DeviceHardWareId} already exists in database");

                Microsoft.Azure.Devices.Device device = await _deviceRegistryService.CreateAndRegisterDevice(registryRequest);
                string assignedId = Guid.NewGuid().ToString();
                DeviceRegistryResponse response = new DeviceRegistryResponse() {
                    AssignedId = assignedId,
                    X509Thumbprint = device.Authentication.X509Thumbprint.PrimaryThumbprint
                };
                
                Models.Devices.Device modelDevice = new Models.Devices.Device()
                {
                    Id = assignedId,
                    HardwareId = registryRequest.DeviceHardWareId,
                    X509PrimaryThumbprint = device.Authentication.X509Thumbprint.PrimaryThumbprint,
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
        [Route("devicesForGrouping")]
        public async Task<IActionResult> GetDevicesForGrouping([FromQuery] string groupingName, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
                if (user == null)
                    return Unauthorized("Invalid refresh token");

                user = await _dbContext.Users.Include("Groupings").FirstOrDefaultAsync(x => x.Id == user.Id);
                Grouping grouping = user.Groupings.FirstOrDefault(x => x.Name.ToLower() == groupingName.ToLower());

                if (grouping.Devices == null)
                {
                    return Ok(new DeviceQueryResponse() { Devices = new List<DeviceDTO>() { }, TotalCount = 0 });

                }

                // Calculate the number of pages
                int totalPages = (int)Math.Ceiling((double)grouping.Devices.Count / pageSize);

                // Get the users for the specified page
                var devices = await _dbContext.Devices
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                List<DeviceDTO> deviceDTOs = _mapper.Map<List<Models.Devices.Device>, List<DeviceDTO>>(devices);

                return Ok(new DeviceQueryResponse() { Devices = deviceDTOs, TotalCount = grouping.Devices.Count });
            }
            catch (Exception e)
            {
                return BadRequest(e);
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
