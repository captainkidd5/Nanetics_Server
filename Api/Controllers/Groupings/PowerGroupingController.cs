using AutoMapper;
using DatabaseServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using Models.Phones;
using Api.DependencyInjections.Authentication;
using Models.GroupingStuff;
using Contracts.GroupingStuff;

namespace Api.Controllers.groupings
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("[controller]")]
    public class PowerGroupingController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAuthManager _authManager;
        private readonly ILogger<PowerGroupingController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _iMapper;

        public PowerGroupingController(AppDbContext appDbContext, IAuthManager authManager,ILogger<PowerGroupingController> logger, UserManager<ApplicationUser> userManager, IMapper iMapper)
        {
            _appDbContext = appDbContext;
            _authManager = authManager;
            _logger = logger;
            _userManager = userManager;
            _iMapper = iMapper;
        }


        [HttpPost]
        [Route("creategroupingForUser")]

        public async Task<IActionResult> CreategroupingForUser([FromBody] GroupingRegistrationRequest groupingRegistrationRequest, [FromQuery] string userId)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            user = await _appDbContext.Users.Include("groupinges").FirstOrDefaultAsync(x => x.Id == user.Id);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            if (user.Groupings != null && user.Groupings.Count > 0)
                return BadRequest("User has max grouping count");

            if (user.Groupings == null)
                user.Groupings = new List<Grouping>();
            bool exists = await _appDbContext.Groupings.FirstOrDefaultAsync(x => x.Name.ToLower() == groupingRegistrationRequest.Name.ToLower()) != null;

            if (exists)
                return BadRequest("Name taken");

            Grouping grouping = _iMapper.Map<Grouping>(groupingRegistrationRequest);

            grouping.Id = Guid.NewGuid().ToString();
            grouping.User = user;
           // grouping.UserId = user.Id;
            user.Groupings.Add(grouping);
            await _userManager.UpdateAsync(user);

            //var result = await _appDbContext.groupinges.AddAsync(grouping);
            // await _appDbContext.SaveChangesAsync();

            return Ok();


        }

        [HttpPut]
        [Route("updategroupingForUser")]

        public async Task<IActionResult> Updategrouping([FromBody] GroupingUpdateRequest groupingUpdateRequest)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Update grouping on database ATTEMPT for " +
         "grouping name {GroupingName} BY {User}",
     HttpContext.GetEndpoint(),
              HttpContext.Request.Method,
              groupingUpdateRequest.Name,
              user.Email);
            Grouping grouping = _appDbContext.Groupings.FirstOrDefault(x => x.Id == groupingUpdateRequest.Id);
            if (grouping == null)
                return BadRequest($"Grouping with id {groupingUpdateRequest.Id} not found");

            // Update the Name and Description properties of the existing grouping object
            grouping.Name = groupingUpdateRequest.Name ?? grouping.Name;
            grouping.Description = groupingUpdateRequest.Description ?? grouping.Description;

            _appDbContext.Update(grouping);
            var result = await _appDbContext.SaveChangesAsync();
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Update grouping on database SUCCESS for " +
         "grouping name {GroupingName} BY {User}",
     HttpContext.GetEndpoint(),
              HttpContext.Request.Method,
              groupingUpdateRequest.Name,
              user.Email);
            return Ok(result);

        }


        /// <summary>
        /// User must be owner of grouping to delete it
        /// </summary>
        /// <param name="groupingId"></param>
        /// <returns></returns>

        [HttpDelete]
        [Route("deletegroupingForUser")]
        public async Task<IActionResult> Deletegrouping([FromQuery] string groupingId)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Delete grouping on database ATTEMPT for " +
         "grouping id {GroupingId} BY {User}",
     HttpContext.GetEndpoint(),
              HttpContext.Request.Method,
            groupingId,
              user.Email);
            List<Grouping> groupinges = _appDbContext.Users.Include("Groupings").FirstOrDefaultAsync(x => x.Id == user.Id).Result.Groupings;

            Grouping grouping = groupinges.FirstOrDefault(x => x.Id == groupingId);

            if (grouping == null)
                return NotFound($"User does not have grouping with id{groupingId}");

            _appDbContext.Groupings.Remove(grouping);
            var result = await _appDbContext.SaveChangesAsync();
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Delete grouping on database SUCCESS for " +
       "grouping id {GroupingId} BY {User}",
   HttpContext.GetEndpoint(),
            HttpContext.Request.Method,
          groupingId,
            user.Email);
            return Ok(result);
        }


    }
}
