using DatabaseServices;
using SilverMenu.DependencyInjections.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models.Authentication;
using Models.Devices;

namespace SilverMenu.Controllers.Devices
{
    [ApiController]
    [Route("[controller]")]

    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAuthManager _authManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DevicesController(AppDbContext appDbContext, IAuthManager authManager, UserManager<ApplicationUser> userManager)
        {
            _appDbContext = appDbContext;
            _authManager = authManager;
            _userManager = userManager;
        }


        [HttpGet]
        [Route("doesExist")]

        public async Task<IActionResult> DoesExist([FromBody] string expoToken)
        {
            bool exists =  _appDbContext.Users.Any(x => x.Devices.FirstOrDefault(x => x.ExpoPushToken == expoToken) != null);

            return Ok(exists);

        }

        [HttpPost]
        [Route("create")]

        public async Task<IActionResult> Create([FromBody] DeviceRegistrationRequest deviceRegistrationRequest)
        {

            try
            {

            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            user = await _userManager.Users.Include("Words").FirstOrDefaultAsync(x => x.UserName.Equals(user.UserName));

            if (user == null)
            {
                return NotFound("Unable to find user");
            }





            if (string.IsNullOrEmpty(deviceRegistrationRequest.Token))
                return BadRequest("Invalid token format");


            if(!deviceRegistrationRequest.Token.Contains("ExponentPushToken["))
                return BadRequest("Invalid token format");

            ApplicationUser foundUser = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Devices.Any(y => y.ExpoPushToken.Equals(deviceRegistrationRequest.Token)));

            if(foundUser == null)
            {
                if (user.Devices == null)
                    user.Devices = new List<Device>();
                user.Devices.Add(new Device() { Id = Guid.NewGuid().ToString(), ExpoPushToken = deviceRegistrationRequest.Token, TimeZone = deviceRegistrationRequest.Calendar.TimeZone,UserId = user.Id.ToString(), ApplicationUser =user});
                await _userManager.UpdateAsync(user);
                return Ok("New device registered to user");
            }
            return Ok("Device already registered");


            }
            catch (Exception ex)
            {
                return StatusCode(500);

            }
        }
    }
}
