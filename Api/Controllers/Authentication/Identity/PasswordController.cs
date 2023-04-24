using AutoMapper;
using Contracts.Authentication.Identity.Create;
using DatabaseServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Authentication;
using Api.DependencyInjections.Authentication;
using Api.DependencyInjections.Email;
using System.Security.Claims;
using System.Web;

namespace Api.Controllers.Authentication.Identity
{

    [ApiController]
    [Route("[controller]")]
    public class PasswordController : Controller
    {

        private readonly ILogger<AccountController> _logger;
        private readonly IAuthManager _authManager;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public PasswordController(ILogger<AccountController> logger,
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
        [Route("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);


            var token = await _userManager.GeneratePasswordResetTokenAsync(user);


            var emailResult = await _emailSender.SendPasswordResetEmail(user.Email, user.Id.ToString(), token);

            if (emailResult.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Password reset email sent to {user.Email}", null);
                return Accepted();

            }

            return BadRequest(emailResult.StatusCode);



        }

        [HttpPost()]
        [Route("confirmForgotPasswordReset")]
        public async Task<IActionResult> ConfirmForgotPasswordReset([FromBody] ForgotPasswordResetRequest passwordResetModel)
        {

            var user = await _userManager.FindByEmailAsync(passwordResetModel.Email);
            if (user == null)

                return NotFound($"Unable to load user with email '{passwordResetModel.Email}'.");
            var decodedToken = HttpUtility.UrlDecode(passwordResetModel.Token).Replace(" ", "+");
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, passwordResetModel.Password);
            if (!result.Succeeded)
                return BadRequest();

            return Ok();

        }



        [HttpPost]
        [Route("changePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");


            IdentityResult resetPassResult = await _userManager.ChangePasswordAsync(user, changePasswordRequest.OldPassword, changePasswordRequest.NewPassword);

            if (!resetPassResult.Succeeded)
                return BadRequest();

            return Ok();
        }
    }

}
