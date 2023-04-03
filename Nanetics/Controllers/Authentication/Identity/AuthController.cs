using AutoMapper;
using Contracts.Authentication;
using Contracts.Authentication.Identity.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Models.Authentication;
using Services;
using System.Security.Claims;
using SilverMenu.DependencyInjections.Authentication;
using DatabaseServices;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using SilverMenu.DependencyInjections.Email;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Web;

namespace SilverMenu.Controllers.Authentication.Identity
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {

        private readonly ILogger<AccountController> _logger;
        private readonly IAuthManager _authManager;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AuthController(ILogger<AccountController> logger,
            IMapper mapper, IAuthManager authManager, AppDbContext dbContext, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _logger = logger;
            _authManager = authManager;
            _dbContext = dbContext;
            _mapper = mapper;
            _userManager = userManager;
            _emailSender = emailSender;

        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] LoginUserDTO loginUserDTO)
        {

            _logger.LogInformation($"Registration attempt for {loginUserDTO.Email}");

            //ModelState refers to the data attributes above model properties
            if (!ModelState.IsValid)
            {
                _logger.LogError("Registration: bad modelstate");

                return BadRequest();
            }
            try
            {
                ApplicationUser usr = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email.Equals(loginUserDTO.Email));
                if (usr != null && usr.EmailConfirmed)
                {
                    _logger.LogError("Email already in use", usr.Email);

                    return BadRequest("Email already in use.");
                }
                else if (usr != null)
                {
                    //User exists but no confirmation email yet

                    return await SendConfirmationEmail(loginUserDTO, usr);
                }
                UserDTO userDto = new UserDTO() { Email = loginUserDTO.Email, Password = loginUserDTO.Password };
                ApplicationUser user = _mapper.Map<ApplicationUser>(userDto);

                user.UserName = loginUserDTO.Email.ToLower();
                user.Id = Guid.NewGuid();
                user.ConfirmationCode = "000000";
                var result = await _userManager.CreateAsync(user, loginUserDTO.Password);

                if (!result.Succeeded)
                {
                    string errors = string.Empty;
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                        errors += error.Description + "/n";
                    }

                    _logger.LogError("Registration error for " + userDto.Email + errors, errors);

                    return BadRequest(ModelState);
                }

                await _userManager.AddToRolesAsync(user, new List<string>()
                {
                    "User"
                });

                return await SendConfirmationEmail(loginUserDTO, user);

            }
            catch (Exception e)
            {
                string errorMsg = $"Something went wrong in the {nameof(Register)}";
                _logger.LogError(e, errorMsg);

                return Problem(e.ToString(), statusCode: 500);
            }

        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("deleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromBody] LoginUserDTO loginUserDTO)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                if (!await _authManager.ValidateUser(loginUserDTO))
                {
                    return BadRequest();
                }
                ApplicationUser user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == loginUserDTO.Email);
                if (user == null)
                    return Unauthorized("Invalid User");

                if (!loginUserDTO.Email.Equals(user.Email))
                    return BadRequest();


                _logger.LogInformation($"Deletion attempt for {loginUserDTO.Email}");

                //ModelState refers to the data attributes above model properties

                var rolesForUser = await _userManager.GetRolesAsync(user);
                if (rolesForUser.Count() > 0)
                {
                    foreach (var item in rolesForUser.ToList())
                    {
                        // item should be the name of the role
                        var roleRemovalResult = await _userManager.RemoveFromRoleAsync(user, item);
                    }
                }

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }



                return Ok();

            }
            catch (Exception e)
            {
                string errorMsg = $"Something went wrong in the {nameof(DeleteAccount)}";
                _logger.LogError(e, errorMsg);

                return Problem(e.ToString(), statusCode: 500);
            }

        }
        

        [HttpPost()]
        [Route("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string code)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            if (user.ConfirmationCode.Equals(code))
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                return Ok();

            }

            return BadRequest("Unable to verify email.");
        }

        private string GenerateConfirmationCode(int digits = 6)
        {
            Random r = new Random();
            int randNum = r.Next(1000000);
            string sixDigitNumber = randNum.ToString("D" + digits.ToString());

            return sixDigitNumber;
        }
        private async Task<IActionResult> SendConfirmationEmail(LoginUserDTO userDTO, ApplicationUser user)
        {
            try
            {
                var code = GenerateConfirmationCode();

                user.ConfirmationCode = code;
                await _userManager.UpdateAsync(user);




                var emailResult = await _emailSender.SendEmailConfirmation(user.Email, user.Id.ToString(), code);

                if (emailResult.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Confirmation email sent to {user.Email}", null);
                    return Accepted(userDTO);

                }

                return BadRequest(emailResult.StatusCode);

            }
            catch (Exception e)
            {
                _logger.LogInformation($"Unable to send confirmation email to {user.Email}", null);

                return BadRequest(e.ToString());
            }


        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO loginUserDTO)
        {
            _logger.LogWarning($"Login attempt for {loginUserDTO.Email}");

            if (!ModelState.IsValid)
            {
                return BadRequest(loginUserDTO);
            }

            try
            {

                if (!await _authManager.ValidateUser(loginUserDTO))
                {
                    return Unauthorized();
                }
                ApplicationUser user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == loginUserDTO.Email);
                if (user == null)
                    return Unauthorized();

                if (!user.EmailConfirmed)
                    return Unauthorized("Please confirm your email address");
                //TODO: Make days part of config
                user.RefreshToken = await _authManager.CreateToken(user, true);
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
                await _userManager.UpdateAsync(user);
                Response.Cookies.Append("refreshToken", await _authManager.CreateToken(user, true),
                    new CookieOptions()
                    {
                        Expires = DateTimeOffset.Now.AddDays(3),
                        HttpOnly = true,
                        IsEssential = true
                    });

                return Accepted(new { Token = await _authManager.CreateToken(user, false) });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Something went wrong in name of {nameof(Login)}");
                return Problem(e.ToString(), statusCode: 500);
            }
        }

        [HttpGet]
        [Route("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            //TODO: Handle edge cases found here:
            //https://github.com/gitdagray/nodejs_jwt_auth/blob/main/controllers/refreshTokenController.js


            string refreshVal = string.Empty;


            if (!Request.Cookies.TryGetValue("refreshToken", out refreshVal))
                return BadRequest();


            ClaimsPrincipal claim = _authManager.VerifyIfValidToken(refreshVal);
            if (claim == null)
                return BadRequest("Invalid access token or refresh token");


            var user = await _userManager.FindByNameAsync(claim.Identity.Name);

            if (user == null || user.RefreshToken == null
                || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token or refresh token");
            }
            if (user.RefreshToken.ToLower() == refreshVal.ToLower())
            {
                //reuse
            }




            string newToken = await _authManager.CreateToken(user, false);
            string newRefresh = await _authManager.CreateToken(user, true);

            //Refresh token is sent only sent back as cookie (to prevent js access)

            Response.Cookies.Append("refreshToken", newRefresh,
                new CookieOptions()
                {
                    Expires = DateTimeOffset.Now.AddDays(3),
                    HttpOnly = true,
                    IsEssential = true
                });

            Response.Cookies.Append("Authorization", newToken,
                new CookieOptions() { Expires = DateTimeOffset.Now.AddMinutes(2), IsEssential = true });


            user.RefreshToken = newRefresh;
            user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(120);
            await _userManager.UpdateAsync(user);
            return Ok(new { Token = await _authManager.CreateToken(user, false) });

        }


        [Authorize]
        [HttpPost]
        [Route("revoke")]
        public async Task<IActionResult> Revoke([FromBody] DeleteUserDTO userDTO)
        {
            var user = await _userManager.FindByNameAsync(userDTO.Email);
            if (user == null) return BadRequest("Invalid user name");

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return Ok();
        }

        [Authorize]
        [HttpPost]
        [Route("revoke-all")]
        public async Task<IActionResult> RevokeAll()
        {
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                user.RefreshToken = null;
                await _userManager.UpdateAsync(user);
            }

            return NoContent();
        }

    }
}
