using Api.DependencyInjections.Authentication;
using AutoMapper;
using Contracts.Authentication.Identity.Create;
using Contracts.Devices;
using Contracts.GroupingStuff;
using DatabaseServices;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ILogger<Device> _logger;
        private readonly IAuthManager _authManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDeviceRegistryService _deviceRegistryService;

        public DevicesController(AppDbContext dbContext, IMapper mapper, ILogger<Device> logger, IAuthManager authManager, UserManager<ApplicationUser> userManager, IDeviceRegistryService deviceRegistryService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
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
        /// Returns true if database contains a device with the given hardware id
        /// </summary>
        /// <param name="hardwareId"></param>
        /// <returns></returns>
        /// 
        [Authorize("LoggedIn")]


        [HttpGet]
        [Route("isRegistered")]
        public async Task<IActionResult> IsRegistered([FromQuery] ulong hardwareId)
        {
            ApplicationUser user = await _authManager.VerifyAccessTokenAndReturnuser(Request, User);
            if (user == null)
                return Unauthorized("Invalid access token");

            Device d = await _dbContext.Devices.FirstOrDefaultAsync(x => x.HardwareId == hardwareId);
            bool isRegistered = d != null;

            return Ok(new { IsRegistered = isRegistered });

        }

        /// <summary>
        /// Device is plugged in, if device does NOT have a device id and x509 certificate, then ping api for a new one. Will both
        /// register the device in IoT and in db (without owner)
        /// </summary>
        /// <returns></returns>
        /// 
        [Authorize("LoggedIn")]
        [HttpPost]
        [Route("RegistrationRequest")]
        public async Task<IActionResult> RegistrationRequest([FromBody] DeviceRegistryRequest registryRequest)
        {

            ApplicationUser user = await _authManager.VerifyAccessTokenAndReturnuser(Request, User);
            if (user == null)
                return Unauthorized("Invalid access token");
            user = await _dbContext.Users.Include("Groupings").Include("Devices").FirstOrDefaultAsync(x => x.Id == user.Id);

            var devices = await _dbContext.Devices.ToListAsync();
            if (await _dbContext.Devices.FirstOrDefaultAsync(x => x.HardwareId == registryRequest.DeviceHardWareId) != null)
            {
                string erMsg = $"Device with hardware id {registryRequest.DeviceHardWareId} already exists in database";
                _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Device Registration on database FAILURE for" +
                erMsg,
                        HttpContext.GetEndpoint(),
                                 HttpContext.Request.Method,
                                 registryRequest.DeviceHardWareId);

                return BadRequest(erMsg);
            }

            Microsoft.Azure.Devices.Device device = await _deviceRegistryService.CreateAndRegisterDevice(registryRequest);
            DeviceRegistryResponse response = new DeviceRegistryResponse()
            {
                AssignedId = device.Id,
                X509Thumbprint = device.Authentication.X509Thumbprint.PrimaryThumbprint
            };


            Models.Devices.Device modelDevice = new Models.Devices.Device()
            {
                Id = device.Id,
                //GroupingId = "888d2c00-661e-490f-847e-734d6805029d",
                HardwareId = registryRequest.DeviceHardWareId,
                X509PrimaryThumbprint = device.Authentication.X509Thumbprint.PrimaryThumbprint,
                GenerationId = 1,
                ETag = device.ETag,
                ConnectionState = DeviceConnectionState.Disconnected,
                Status = DeviceStatus.Disabled,
                CloudToDeviceMessageCount = 1,



            };
            Grouping userBaseGrouping = user.Groupings.FirstOrDefault(x => x.IsBaseGrouping);
            userBaseGrouping.Devices.Add(modelDevice);
            _dbContext.Update(userBaseGrouping);

            if (user.Devices == null)
                user.Devices = new List<Device>();
            user.Devices.Add(modelDevice);
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();



            return Ok(response);


        }


        [HttpGet]
        [Route("devicesForGrouping")]
        public async Task<IActionResult> GetDevicesForGrouping([FromQuery] string groupingName, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                ApplicationUser user = await _authManager.VerifyAccessTokenAndReturnuser(Request, User);
                if (user == null)
                    return Unauthorized("Invalid access token");

                user = await _dbContext.Users.Include("Groupings").Include("Devices").FirstOrDefaultAsync(x => x.Id == user.Id);
                Grouping grouping = user.Groupings.FirstOrDefault(x => x.Name.ToLower() == groupingName.ToLower());

                if (grouping.Devices == null)
                {
                    return Ok(new DeviceQueryResponse() { Devices = new List<DeviceDTO>() { }, TotalCount = 0 });

                }

                // Calculate the number of pages
                int totalPages = (int)Math.Ceiling((double)grouping.Devices.Count / pageSize);

                // Get the users for the specified page
                var devices = grouping.Devices
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize).ToList();

                List<DeviceDTO> deviceDTOs = _mapper.Map<List<Models.Devices.Device>, List<DeviceDTO>>(devices);

                return Ok(new DeviceQueryResponse() { Devices = deviceDTOs, TotalCount = grouping.Devices.Count });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        }
        [HttpGet]
        [Route("devicesWithoutGrouping")]
        public async Task<IActionResult> GetDevicesWithoutGrouping([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                ApplicationUser user = await _authManager.VerifyAccessTokenAndReturnuser(Request, User);
                if (user == null)
                    return Unauthorized("Invalid access token");

                user = await _dbContext.Users.Include("Devices").FirstOrDefaultAsync(x => x.Id == user.Id);



                if (user.Devices == null || user.Devices.Count == 0)
                {
                    return Ok(new DeviceQueryResponse() { Devices = new List<DeviceDTO>() { }, TotalCount = 0 });

                }

                // Calculate the number of pages
                int totalPages = (int)Math.Ceiling((double)user.Devices.Count / pageSize);

                // Get the users for the specified page
                var devices = await _dbContext.Devices
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                List<DeviceDTO> deviceDTOs = _mapper.Map<List<Models.Devices.Device>, List<DeviceDTO>>(devices);

                return Ok(new DeviceQueryResponse() { Devices = deviceDTOs, TotalCount = user.Devices.Count });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        }

        [HttpPut]
        [Route("updateDevice")]

        public async Task<IActionResult> UpdateDevice([FromBody] DeviceUpdateRequest deviceUpdateRequest)
        {
            ApplicationUser user = await _authManager.VerifyAccessTokenAndReturnuser(Request, User);
            if (user == null)
                return Unauthorized("Invalid access token");

            user = await _dbContext.Users.Include("Groupings").Include("Devices").FirstOrDefaultAsync(x => x.Id == user.Id);
            Device device = await _dbContext.Devices.FirstOrDefaultAsync(x => x.Id == deviceUpdateRequest.Id);
            if (!string.IsNullOrEmpty(device.Nickname) && device.Nickname != deviceUpdateRequest.Nickname)
            {
                if (user.Devices.FirstOrDefault(x => x.Nickname.ToLower() == deviceUpdateRequest.Nickname.ToLower()) != null)
                {
                    return BadRequest("Nickname already exists, please choose another");
                }
            }
            if (device == null)
                return NotFound();

            //Remove device from its original grouping if a request was made to change it
            if (device.Grouping != null && device.Grouping.Id != deviceUpdateRequest.Id)
            {
                Grouping originalDeviceGrouping = await _dbContext.Groupings.Include("Devices")
                    .FirstOrDefaultAsync(x => x.Id == deviceUpdateRequest.GroupingId);

                originalDeviceGrouping.Devices.Remove(device);
                _dbContext.Update(originalDeviceGrouping);
            }

            if (!string.IsNullOrEmpty(deviceUpdateRequest.GroupingId))
            {
                //We are moving the device to another grouping, not just completely removing its grouping
                Grouping grouping = user.Groupings.FirstOrDefault(x => x.Id == deviceUpdateRequest.GroupingId);
                if (grouping == null)
                    return BadRequest("User does not own specified grouping");
                _dbContext.Entry(grouping).Collection(g => g.Devices).Load();
                device.Nickname = deviceUpdateRequest.Nickname;

                //Grouping does not contain device, add it
                if (grouping.Devices.FirstOrDefault(x => x.Id == deviceUpdateRequest.Id) == null)
                    grouping.Devices.Add(device);
                _dbContext.Update(grouping);
            }





            var result = await _dbContext.SaveChangesAsync();

            return Ok(result);

        }
        [HttpGet]
        [Route("GetData")]
        public async Task<IActionResult> GetData()
        {
            return Ok(new { Message = "test" });
        }


    }
}
