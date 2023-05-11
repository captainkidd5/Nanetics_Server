using AutoMapper;
using Contracts.Authentication;
using Contracts.Authentication.Identity.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Models.Authentication;
using System.Security.Claims;
using Api.DependencyInjections.Authentication;
using DatabaseServices;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Api.DependencyInjections.Email;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Web;
using static Api.DependencyInjections.Authentication.AuthManager;
using System;

namespace Api.Controllers.Authentication.Identity
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

            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Registration on database ATTEMPT for {email}",
                   HttpContext.GetEndpoint(),
                            HttpContext.Request.Method,
                            loginUserDTO.Email);

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
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Registration on database SUCCESS for {email}," +
                "Email confirmation sent",
                 HttpContext.GetEndpoint(),
                          HttpContext.Request.Method,
                          loginUserDTO.Email);

            IActionResult res = await SendConfirmationEmail(loginUserDTO, user);
            UserDTO dto = res as UserDTO;
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Registration on database SUCCESS for {email}," +
                "email sent.",
                HttpContext.GetEndpoint(),
                         HttpContext.Request.Method,
                         loginUserDTO.Email);
            return Ok(dto);

 

        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("deleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromBody] LoginUserDTO loginUserDTO)
        {
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Delete Account on database ATTEMPT for {email}",
               HttpContext.GetEndpoint(),
                        HttpContext.Request.Method,
                        loginUserDTO.Email);
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


            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Delete Account on database SUCCESS for {email}",
     HttpContext.GetEndpoint(),
              HttpContext.Request.Method,
              loginUserDTO.Email);
            return Ok();


        }


        [HttpPost()]
        [Route("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string code)
        {

            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Confirm email on database attempt for {email}",
               HttpContext.GetEndpoint(),
                        HttpContext.Request.Method,
                        email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            if (user.ConfirmationCode.Equals(code))
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Confirm email on database SUCCESS for {email}",
                   HttpContext.GetEndpoint(),
                            HttpContext.Request.Method,
                            email);
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
                ApplicationUser caller = await _authManager.VerifyAccessTokenAndReturnuser(Request,User);

            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Confirmation Email send ATTEMPT for {email}" +
                " BY caller {caller}",
                      HttpContext.GetEndpoint(),
                               HttpContext.Request.Method,
                               user.Email, caller.Email);
                var code = GenerateConfirmationCode();

                user.ConfirmationCode = code;
                await _userManager.UpdateAsync(user);




                var emailResult = await _emailSender.SendEmailConfirmation(user.Email, user.Id.ToString(), code);

                if (emailResult.IsSuccessStatusCode)
                {
                _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Confirmation Email send SUCCESS for {email} " +
                    " BY caller {caller}",
                  HttpContext.GetEndpoint(),
                           HttpContext.Request.Method,
                           user.Email,caller.Email);
                return Accepted(userDTO);

                }

                return BadRequest(emailResult.StatusCode);

            
       

        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO loginUserDTO)
        {
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Login ATTEMPT for login user {email}",
                   HttpContext.GetEndpoint(),
                            HttpContext.Request.Method,
                            loginUserDTO.Email);


                if (!await _authManager.ValidateUser(loginUserDTO))
                {
                    return Unauthorized();
                }
                ApplicationUser user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == loginUserDTO.Email);
                if (user == null)
                    return Unauthorized();

                if (!user.EmailConfirmed)
                    return Unauthorized("Please confirm your email address");

                user.RefreshToken = await _authManager.CreateToken(user, true);
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
                await _userManager.UpdateAsync(user);
            //RefreshToken is returned as a header
                Response.Cookies.Append("refreshToken", user.RefreshToken,
                    new CookieOptions()
                    {
                        Expires = user.RefreshTokenExpiryTime,
                        HttpOnly = true,
                        IsEssential = true
                    });
                _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Login SUCCESS for login user {email}",
                       HttpContext.GetEndpoint(),
                                HttpContext.Request.Method,
                                loginUserDTO.Email);
            //return ACCESS token in json payload
                return Accepted(new { Token = await _authManager.CreateToken(user, false) });

        }

        [HttpGet]
        [Route("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            //TODO: Handle edge cases found here:
            //https://github.com/gitdagray/nodejs_jwt_auth/blob/main/controllers/refreshTokenController.js

            RefreshTokenStatusResponse tokenResponse = await _authManager.RefreshToken(Request, Response);
         

            if(tokenResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Response.Cookies.Append("Authorization", tokenResponse.Token,
                    new CookieOptions() { Expires = DateTimeOffset.Now.AddMinutes(2), IsEssential = true });
                return Ok(tokenResponse.ErrorMessage);

            }
            else
            {
                _logger.LogError("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Exception: {exception}",
   HttpContext.GetEndpoint(),
                  HttpContext.Request.Method,
                  tokenResponse.ErrorMessage);
                return BadRequest(tokenResponse.ErrorMessage);

            }

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
