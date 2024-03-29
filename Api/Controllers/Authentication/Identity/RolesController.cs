﻿using Api.DependencyInjections.Authentication;
using AutoMapper;
using Contracts.Authentication.Identity.Create;
using Contracts.Authentication.Identity.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using System.Runtime.InteropServices;

namespace Api.Controllers.Authentication.Identity
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<RolesController> _logger;
        private readonly IAuthManager _authManager;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _usersManager;

        public RolesController(RoleManager<ApplicationRole> roleManager,ILogger<RolesController> logger,IAuthManager authManager, IMapper mapper, UserManager<ApplicationUser> usersManager)
        {
            _roleManager = roleManager;
            _logger = logger;
            _authManager = authManager;
            _mapper = mapper;
            _usersManager = usersManager;
        }

        [Authorize]
        [HttpGet()]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Route("")]
        public async Task<IActionResult> GetRoles()
        {

            var roles = await _roleManager.Roles.ToListAsync();
            List<RoleDTO> roleDTOs = _mapper.Map<List<RoleDTO>>(roles);
            return Ok(roleDTOs);
        }

        [Authorize]
        [HttpGet()]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Route("getRoleInfo")]
        public async Task<IActionResult> GetRoleInfo([FromQuery] string roleName)
        {
            ApplicationRole role = await _roleManager.Roles.FirstOrDefaultAsync(x => x.Name.Equals(roleName));
            if (role == null)
                return BadRequest();

            IList<ApplicationUser> usersInRole = await _usersManager.GetUsersInRoleAsync(roleName);

            List<string> userNames = usersInRole.Select(x => x.Email).ToList();
            return Ok(new RoleInfoQueryResponse { Name = roleName, Users = userNames });
        }

        [Authorize]
        [HttpPost()]

        [Route("create")]
        public async Task<IActionResult> Create([FromQuery] string name)
        {
            ApplicationUser caller = await _authManager.VerifyAccessTokenAndReturnuser(Request,User);

            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message:ATTEMPT to create role {role} BY caller {caller}",
     HttpContext.GetEndpoint(),
        HttpContext.Request.Method,
        name, caller.Email);
            ApplicationRole role = new ApplicationRole();
            role.Name = name;

            IdentityResult result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
                return Ok();
            else
                return BadRequest();
        }

        [Authorize]
        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] RoleDTO roleDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (!(await _roleManager.RoleExistsAsync(roleDTO.Name)))
            {
                return BadRequest($"Role {roleDTO.Name} does not exist");
            }

            List<ApplicationUser> usersWithRole = (List<ApplicationUser>)await _usersManager.GetUsersInRoleAsync(roleDTO.Name);

            foreach (ApplicationUser user in usersWithRole)
                await _usersManager.RemoveFromRoleAsync(user, roleDTO.Name);

            ApplicationRole role = await _roleManager.Roles.FirstOrDefaultAsync(x => x.Name == roleDTO.Name);
            if (role == null)
                return BadRequest();

            IdentityResult result = await _roleManager.DeleteAsync(role);




            if (result.Succeeded)
                return Ok();
            else
                return BadRequest();
        }
    }
}
