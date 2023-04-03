using AutoMapper;
using Contracts.BusinessStuff;
using DatabaseServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using Models.BusinessStuff;
using Models.Devices;
using SilverMenu.DependencyInjections.Authentication;

namespace SilverMenu.Controllers.Businesses
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("[controller]")]
    public class PowerBusinessController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAuthManager _authManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _iMapper;

        public PowerBusinessController(AppDbContext appDbContext, IAuthManager authManager, UserManager<ApplicationUser> userManager, IMapper iMapper)
        {
            _appDbContext = appDbContext;
            _authManager = authManager;
            _userManager = userManager;
            _iMapper = iMapper;
        }


        [HttpPost]
        [Route("createBusinessForUser")]

        public async Task<IActionResult> CreateBusinessForUser([FromBody] BusinessRegistrationRequest businessRegistrationRequest, [FromQuery] string userId)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            user = await _appDbContext.Users.Include("Businesses").FirstOrDefaultAsync(x => x.Id == user.Id);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            if (user.Businesses != null && user.Businesses.Count > 0)
                return BadRequest("User has max business count");

            if (user.Businesses == null)
                user.Businesses = new List<Business>();
            bool exists = await _appDbContext.Businesses.FirstOrDefaultAsync(x => x.Name.ToLower() == businessRegistrationRequest.Name.ToLower()) != null;

            if (exists)
                return BadRequest("Name taken");

            Business business = _iMapper.Map<Business>(businessRegistrationRequest);

            business.Id = Guid.NewGuid().ToString();
            business.User = user;
            business.UserId = user.Id;
            user.Businesses.Add(business);
            await _userManager.UpdateAsync(user);

            //var result = await _appDbContext.Businesses.AddAsync(business);
            // await _appDbContext.SaveChangesAsync();

            return Ok();


        }

        [HttpPut]
        [Route("updateBusinessForUser")]

        public async Task<IActionResult> UpdateBusiness([FromBody] BusinessUpdateRequest businessUpdateRequest)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            Business business = _appDbContext.Businesses.FirstOrDefault(x => x.Id == businessUpdateRequest.Id);
            if (business == null)
                return BadRequest($"Business with id {businessUpdateRequest.Id} not found");

            // Update the Name and Description properties of the existing Business object
            business.Name = businessUpdateRequest.Name ?? business.Name;
            business.Description = businessUpdateRequest.Description ?? business.Description;

            _appDbContext.Update(business);
            var result = await _appDbContext.SaveChangesAsync();

            return Ok(result);

        }


        /// <summary>
        /// User must be owner of business to delete it
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns></returns>

        [HttpDelete]
        [Route("deleteBusinessForUser")]
        public async Task<IActionResult> DeleteBusiness([FromQuery] string businessId)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            List<Business> businesses = _appDbContext.Users.Include("Businesses").FirstOrDefaultAsync(x => x.Id == user.Id).Result.Businesses;

            Business business = businesses.FirstOrDefault(x => x.Id == businessId);

            if (business == null)
                return NotFound($"User does not have business with id{businessId}");

            _appDbContext.Businesses.Remove(business);
            var result = await _appDbContext.SaveChangesAsync();

            return Ok(result);
        }


    }
}
