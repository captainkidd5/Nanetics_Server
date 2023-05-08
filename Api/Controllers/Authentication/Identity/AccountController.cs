using Microsoft.AspNetCore.Mvc;
using Contracts.Authentication.Identity.Create;
using Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Api.DependencyInjections.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DatabaseServices;
using Api.DependencyInjections.Email;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Collections.Generic;
using Models.GroupingStuff;

namespace Api.Controllers.Authentication.Identity
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly ILogger<AccountController> _logger;
        private readonly IMapper _mapper;
        private readonly IAuthManager _authManager;
        private readonly AppDbContext _dbContext;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
            IUserStore<ApplicationUser> userStore, ILogger<AccountController> logger,
            IMapper mapper, IAuthManager authManager, AppDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _logger = logger;
            _mapper = mapper;
            _authManager = authManager;
            _dbContext = dbContext;
        }

        /// <summary>
        /// THIS IS ADMIN REGISTER ONLY
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("createAccount")]
        public async Task<IActionResult> Register([FromBody] CreateUserDTO userDTO)
        {
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Admin register ATTEMPT for email {email}",
                  HttpContext.GetEndpoint(),
                           HttpContext.Request.Method,
                           userDTO.Email);
            userDTO.Email = userDTO.Email.ToLower();


            //ModelState refers to the data attributes above model properties
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            ApplicationUser usr = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email.Equals(userDTO.Email));
            if (usr != null && usr.EmailConfirmed)
            {
                string errMsg = $"Email already in use.{userDTO.Email}";
                _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Admin register FAILURE for email {email}" +
                    " : " + errMsg,
                 HttpContext.GetEndpoint(),
                          HttpContext.Request.Method,
                          userDTO.Email);
                return BadRequest(errMsg);
            }

            ApplicationUser user = _mapper.Map<ApplicationUser>(userDTO);

            user.UserName = userDTO.Email.ToLower();
            user.Id = Guid.NewGuid();
            user.EmailConfirmed = true;
            user.ConfirmationCode = "";
            user.FirstName = userDTO.FirstName.ToLower();
            user.LastName = userDTO.LastName.ToLower();
            user.PhoneNumber = userDTO.PhoneNumber;
            user.Groupings = new List<Grouping>();
            var result = await _userManager.CreateAsync(user, userDTO.Password);


            await _userManager.AddToRolesAsync(user, new List<string>()
                {
                    "User"
                });
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Admin register SUCCESS for email {email}",
             HttpContext.GetEndpoint(),
                      HttpContext.Request.Method,
                      userDTO.Email);

            return Ok();



        }





        [Authorize(Roles = "Admin")]

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UpdateUserDTO userDTO)
        {

            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Admin update ATTEMPT for user {email}",
             HttpContext.GetEndpoint(),
                      HttpContext.Request.Method,
                      userDTO.Email);
            ApplicationUser user = _userManager.Users.FirstOrDefault(x => x.Id == userDTO.Id);

            if (user == null)
                return NotFound(ModelState);


            user.UserName = userDTO.Email.ToLower();
            user.Email = userDTO.Email.ToLower();
            user.PhoneNumber = userDTO.PhoneNumber;

            user.FirstName = userDTO.FirstName.ToLower();
            user.LastName = userDTO.LastName.ToLower();



            var result = await _userManager.UpdateAsync(user);


            if (!result.Succeeded)
            {

                return BadRequest(ModelState);
            }
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Admin update SUCCESS for user {email}",
             HttpContext.GetEndpoint(),
                      HttpContext.Request.Method,
                      userDTO.Email);
            return Accepted(userDTO);


        }


        [Authorize(Roles = "Admin")]

        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteUserDTO userDTO)
        {
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Admin role deletetion ATTEMPT for user {email}",
         HttpContext.GetEndpoint(),
                  HttpContext.Request.Method,
                  userDTO.Email);
            if (userDTO.Email.Equals("waiikipomm@gmail.com", StringComparison.InvariantCultureIgnoreCase))
                return BadRequest(ModelState);
            var user = _userManager.Users.FirstOrDefault(x => x.Email.Equals(userDTO.Email.ToLower()));



            IdentityResult result = await _userManager.DeleteAsync(user);
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Admin role deletion SUCCESS for user {email}",
         HttpContext.GetEndpoint(),
                  HttpContext.Request.Method,
                  userDTO.Email);
            if (result.Succeeded)
            {

            }


            return Ok(userDTO);

        }

        [Authorize(Roles = "Admin")]
        [HttpGet()]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Route("users")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            ApplicationUser user = await _authManager.VerifyAccessTokenAndReturnuser(Request,User);
               if (user == null)
                return Unauthorized("Invalid access token");
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Get Users ATTEMPT BY user {email}",
HttpContext.GetEndpoint(),
         HttpContext.Request.Method,
         user.Email);
            // Get the total number of users
            int totalUsers = await _userManager.Users.CountAsync();

            // Calculate the number of pages
            int totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            // Get the users for the specified page
            var users = await _userManager.Users.Include("Groupings")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<UserDTO> userDTOs = _mapper.Map<List<ApplicationUser>, List<UserDTO>>(users);


            // Return the paginated results in a JSON object
            return Ok(new UsersQueryResponse() { Users = userDTOs, TotalCount = totalUsers });

        }

        [Authorize(Roles = "Admin")]
        [HttpGet()]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Route("getRoles")]
        public async Task<IActionResult> GetRoles([FromQuery] string email)
        {
            ApplicationUser caller = await _authManager.VerifyAccessTokenAndReturnuser(Request,User);
            if (caller == null)
                return Unauthorized("Invalid access token");
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Get Roles ATTEMPT FOR user {email} " +
                "BY caller {Caller}",
HttpContext.GetEndpoint(),
        HttpContext.Request.Method,
        email, caller.Email);

            ApplicationUser user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.Equals(email));

            if (user == null)
                return BadRequest();

            IList<string> roles = await _userManager.GetRolesAsync(user);
            var usersInRole = await _userManager.GetUsersInRoleAsync("User");
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Get Roles SUCCESS FOR user {email} " +
           "BY caller {Caller}",
HttpContext.GetEndpoint(),
   HttpContext.Request.Method,
   email, caller.Email);
            return Ok(roles);

        }
        [Authorize(Roles = "Admin")]
        [HttpPost()]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Route("addToRole")]
        public async Task<IActionResult> AddToRole([FromQuery] string email, [FromQuery] string roleName)
        {
            ApplicationUser user = await _authManager.VerifyAccessTokenAndReturnuser(Request,User);
            if (user == null)
                return Unauthorized("Invalid access token");
            IList<string> roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(roleName))
                return BadRequest(roles);
            await _userManager.AddToRoleAsync(user, roleName);
            return Ok(roles);

        }
        [Authorize(Roles = "Admin")]
        [HttpPost()]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Route("removeFromRole")]
        public async Task<IActionResult> RemoveFromRole([FromQuery] string email, [FromQuery] string roleName)
        {
            ApplicationUser user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.Equals(email));
            if (user == null)
                return BadRequest();

            IList<string> roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(roleName))
                return BadRequest(roles);
            await _userManager.RemoveFromRoleAsync(user, roleName);
            return Ok(roles);

        }

    }
}
