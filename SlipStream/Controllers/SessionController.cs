using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SlipStream.Data;
using SlipStream.DTOs.Session;
using SlipStream.Services;

namespace SlipStream.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IJWTService _jwtService;
        private readonly Services.ISession _session;

        public SessionController( [FromServices] AppDbContext dbContext, [FromServices] IJWTService jwtService, [FromServices] Services.ISession session)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
            _session = session;
        }

        [HttpPost("", Name = "CreateSession")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionDTO createSessionDTO)
        {
            var result = await _session.CreateSessionAsync(createSessionDTO);
            if (result.Status)
            {
                return Created("", result.Data);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpDelete("{vehicleId}/{sessionId}", Name = "DeleteSession")]
        public async Task<IActionResult> DeleteSession( [FromRoute] int sessionId, [FromRoute] int vehicleId)
        {
            var result = await _session.DeleteSessionAsync(sessionId, vehicleId);
            if (result.Status)
            {
                return NoContent();
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("{vehicleId}/{sessionId}", Name = "GetSession")]
        public async Task<IActionResult> GetSession( [FromRoute] int sessionId, [FromRoute] int vehicleId)
        {
            var result = await _session.GetSessionAsync(sessionId, vehicleId);
            if (result.Status)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("{vehicleId}", Name = "GetVehicleSessions")]
        public async Task<IActionResult> GetVehicleSessions( [FromRoute] int vehicleId)
        {
            var result = await _session.GetVehicleSessionsAsync(vehicleId);
            if (result.Status)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result);
            }
        }

    }
}
