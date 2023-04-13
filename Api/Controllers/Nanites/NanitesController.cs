using Contracts.Devices;
using Microsoft.AspNetCore.Mvc;
using Models.Phones;

namespace Api.Controllers.Gadgets
{

    [ApiController]
    [Route("[controller]")]

    public class NanitesController : ControllerBase
    {
        [HttpPost]
        [Route("ping")]

        public async Task<IActionResult> Ping([FromBody] PingDTO pingDto)
        {
            string msg = pingDto.Message;

            Console.WriteLine(msg);
            return Ok(msg);
        }

        [HttpGet]
        [Route("GetData")]
        public async Task<IActionResult> GetData()
        {
            return Ok(new {Message = "test"});
        }


    }
}
