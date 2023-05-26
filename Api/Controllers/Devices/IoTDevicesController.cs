using Api.DependencyInjections.IoT;
using Contracts.Devices.IoT;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Devices
{
    [ApiController]
    [Route("[controller]")]

    public class IoTDevicesController : ControllerBase 
    {
        private readonly IIotService _ioTService;

        public IoTDevicesController(IIotService iotService)
        {
            _ioTService = iotService;
        }
        [HttpGet]
        [Route("GetTemplates")]
        public async Task<IActionResult> GetTemplates()
        {

            var result = await _ioTService.GetTemplates();
            return Ok(new { Message = "test" });
        }
        [HttpGet]
        [Route("GetIoTDevices")]
        public async Task<IActionResult> GetIotDevices()
        {

            IoTDeviceCollectionDTO result = await _ioTService.GetAllDevices();
            if (result == null)
                return BadRequest();
            return Ok(result);
        }

        /// <summary>
        /// Will return bad request if device has no template
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDeviceComponents")]
        public async Task<IActionResult> GetDeviceComponents([FromQuery] string deviceId)
        {

            IotCollection result = await _ioTService.GetDeviceComponents(deviceId);

            if (result == null)
                return BadRequest();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetDeviceProperties")]
        public async Task<IActionResult> GetDeviceProperties([FromQuery] string deviceId)
        {

            SoilSensorProperties result = await _ioTService.GetDeviceProperties(deviceId);

            if (result == null)
                return BadRequest();
            return Ok(result);
        }
    }
}
