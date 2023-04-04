using AutoMapper;
using Contracts.Authentication.Identity.Create;
using Contracts.Logging;
using DatabaseServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using Models.Logging;
using Services;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class LogsController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public LogsController(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet()]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Route("")]
        public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
           
                // Get the total number of users
                int totalLogs = await _dbContext.Logs.CountAsync();

                // Calculate the number of pages
                int totalPages = (int)Math.Ceiling((double)totalLogs / pageSize);

                // Get the users for the specified page
                List<Models.Logging.Log> logs = await _dbContext.Logs.OrderByDescending(log => log.TimeStamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                List<LogDTO> logDTOs = _mapper.Map<List<LogDTO>>(logs);


                // Return the paginated results in a JSON object
                return Ok(new LogsQueryResponse() { Logs = logDTOs, TotalCount = totalLogs });
 
        }

        [Authorize]
        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteLogDTO logDTO)
        {

            Log log = await _dbContext.Logs.FirstOrDefaultAsync(x => x.Id == logDTO.Id);
            if (log == null) return BadRequest(ModelState);

            try
            {
                _dbContext.Logs.Remove(log);
                return Ok(logDTO);

            }
            catch (Exception e)
            {
                string errorMsg = $"Something went wrong in the {nameof(Delete)}";

                return Problem(errorMsg, statusCode: 500);
            }

        }
        [Authorize]
        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("deleteAll")]
        public async Task<IActionResult> DeleteAll()
        {

            try
            {
                // Get a list of all the logs in the database
                List<Models.Logging.Log> logs = await _dbContext.Logs.ToListAsync();

                // Delete all the logs from the database
                _dbContext.Logs.RemoveRange(logs);
                await _dbContext.SaveChangesAsync();
                return Ok();

            }
            catch (Exception e)
            {
                string errorMsg = $"Something went wrong in the {nameof(DeleteAll)}";

                return Problem(errorMsg, statusCode: 500);
            }

        }

        [Authorize]
        [HttpDelete("deleteMany")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMany([FromBody] List<int> ids)
        {

            if (ids == null)
                return BadRequest();
            List<Log> logs = await _dbContext.Logs.Where(x => ids.Contains((int)x.Id)).ToListAsync();

            _dbContext.Logs.RemoveRange(logs);
            await _dbContext.SaveChangesAsync();
            return Ok();


        }

       
    }
}
