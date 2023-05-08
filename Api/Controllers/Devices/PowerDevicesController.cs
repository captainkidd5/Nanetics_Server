using Api.DependencyInjections.Authentication;
using AutoMapper;
using Contracts.Devices;
using DatabaseServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using System.Data;

namespace Api.Controllers.Devices
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("[controller]")]

    public class PowerDevicesController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IAuthManager _authManager;
        private readonly IMapper _mapper;

        public PowerDevicesController(AppDbContext dbContext, IAuthManager authManager, IMapper mapper)
        {
            _dbContext = dbContext;
            _authManager = authManager;
            _mapper = mapper;
        }
        [HttpGet]
        [Route("devices")]
        public async Task<IActionResult> GetDevices([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            ApplicationUser user = await _authManager.VerifyAccessTokenAndReturnuser(Request,User);
            if (user == null)
                return Unauthorized("Invalid access token");
            int totalDevices = await _dbContext.Devices.CountAsync();

            // Calculate the number of pages
            int totalPages = (int)Math.Ceiling((double)totalDevices / pageSize);

            // Get the users for the specified page
            var devices = await _dbContext.Devices
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<DeviceDTO> deviceDTOs = _mapper.Map<List<Models.Devices.Device>, List<DeviceDTO>>(devices);


            // Return the paginated results in a JSON object
            return Ok(new DeviceQueryResponse() { Devices = deviceDTOs, TotalCount = totalDevices });
        }
    }
}
