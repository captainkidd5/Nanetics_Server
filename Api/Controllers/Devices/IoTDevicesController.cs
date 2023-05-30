using Api.DependencyInjections.Azure;
using Api.DependencyInjections.IoT;
using Contracts.Devices.IoT;
using Contracts.Devices.IoT.Telemetry;
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
        [Route("GetIotTemplates")]
        public async Task<IActionResult> GetIotTemplates()
        {

            var result = await _ioTService.GetIoTTemplates();
            return Ok(new { Message = "test" });
        }
        [HttpGet]
        [Route("GetIoTDevices")]
        public async Task<IActionResult> GetIotDevices()
        {

            IoTDeviceCollectionDTO result = await _ioTService.GetAllIoTDevices();
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
        [Route("GetIoTDeviceComponents")]
        public async Task<IActionResult> GetIoTDeviceComponents([FromQuery] string deviceId)
        {

            IotCollection result = await _ioTService.GetIoTDeviceComponents(deviceId);

            if (result == null)
                return BadRequest();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetIoTDeviceProperties")]
        public async Task<IActionResult> GetIoTDeviceProperties([FromQuery] string deviceId)
        {

            SoilSensorProperties result = await _ioTService.GetIoTDeviceProperties(deviceId);

            if (result == null)
                return BadRequest();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetIoTComponentProperties")]
        public async Task<IActionResult> GetIoTComponentProperties([FromQuery] string deviceId, [FromQuery] string componentName)
        {

            object result = await _ioTService.GetIoTDeviceComponentProperties(deviceId, componentName);

            if (result == null)
                return BadRequest();
            return Ok(result);
        }

        //Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6IndhaWlraXBvbW1AZ21haWwuY29tIiwibmFtZWlkIjoiZGE5ZTI5OGEtMTE4Zi00YTE3LWEyOGUtNDRlOWU1N2NlMjBiIiwidW5pcXVlX25hbWUiOiJ3YWlpa2lwb21tQGdtYWlsLmNvbSIsInJvbGUiOlsiVXNlciIsIkFkbWluIl0sIm5iZiI6MTY4NTMxNTc4MiwiZXhwIjoxNjg1OTIwNTgyLCJpYXQiOjE2ODUzMTU3ODIsImlzcyI6IkFwaSJ9.BrlP1pQyi1uDdc29FLWxN7qO_JNZDlV41L-6aDvBbsc
        [HttpPatch]
        [Route("UpdateIoTComponentProperties")]
        public async Task<IActionResult> UpdateIoTComponentProperties([FromQuery] string deviceId, [FromBody] SoilSensorProperties soilSensorProperties)
        {

            SoilSensorProperties result = await _ioTService.UpdateIoTComponentProperties(deviceId, soilSensorProperties);

            if (result == null)
                return BadRequest();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetIotDeviceTelemetry")]
        public async Task<IActionResult> GetIotDeviceTelemetry([FromQuery] string deviceId, [FromQuery] string telemetryName)
        {

           IoTTelemetry result = await _ioTService.GetTelemetryForDevice(deviceId, telemetryName);

            if (result == null)
                return BadRequest();
            return Ok(result);
        }
        //Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6IndhaWlraXBvbW1AZ21haWwuY29tIiwibmFtZWlkIjoiZGE5ZTI5OGEtMTE4Zi00YTE3LWEyOGUtNDRlOWU1N2NlMjBiIiwidW5pcXVlX25hbWUiOiJ3YWlpa2lwb21tQGdtYWlsLmNvbSIsInJvbGUiOlsiVXNlciIsIkFkbWluIl0sIm5iZiI6MTY4NTQwNTc2OSwiZXhwIjoxNjg2MDEwNTY5LCJpYXQiOjE2ODU0MDU3NjksImlzcyI6IkFwaSJ9.2frKvRCJ7TyHVor9fZIvTEqPCJCMNKAsq2ZTU2bV5c8
        [HttpGet]
        [Route("GetIotDeviceTwin")]
        public async Task<IActionResult> GetIotDeviceTwin([FromQuery] string deviceId)
        {

            Twin result = await _ioTService.GetTwin(deviceId);

            if (result == null)
                return BadRequest();
            return Ok(result);
        }
    }
}
