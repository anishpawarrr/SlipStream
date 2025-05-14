using Microsoft.AspNetCore.Authorization;
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
        private readonly IJWTService _jwtService;
        private readonly Services.ISession _session;
        private const string _authString = "Authorization", _bearerString = "Bearer ";
        private const int _bearerStringLength = 7;


        public SessionController( [FromServices] IJWTService jwtService, [FromServices] Services.ISession session)
        {
            _jwtService = jwtService;
            _session = session;
        }
        
        [HttpPost("", Name = "CreateSession")]
        public async Task<IActionResult> CreateSession( [FromHeader(Name = _authString)] string authorization, [FromBody] CreateSessionDTO createSessionDTO)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith(_bearerString))
            {
                return Unauthorized("Authorization header is missing or invalid");
            }
            string jwtToken = authorization.Substring(_bearerStringLength);
            bool isValidToken = _jwtService.ValidateToken(token: jwtToken, VehicleId: createSessionDTO.VehicleId);
            if (!isValidToken)
            {
                return Unauthorized("Invalid token");
            }
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
        public async Task<IActionResult> DeleteSession( [FromHeader(Name = _authString)] string authorization, [FromRoute] int sessionId, [FromRoute] int vehicleId)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith(_bearerString))
            {
                return Unauthorized("Authorization header is missing or invalid");
            }
            string jwtToken = authorization.Substring(_bearerStringLength);
            bool isValidToken = _jwtService.ValidateToken(token: jwtToken, VehicleId: vehicleId);
            if (!isValidToken)
            {
                return Unauthorized("Invalid token");
            }
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
        public async Task<IActionResult> GetSession( [FromHeader(Name = _authString)] string authorization, [FromRoute] int sessionId, [FromRoute] int vehicleId)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith(_bearerString))
            {
                return Unauthorized("Authorization header is missing or invalid");
            }
            string jwtToken = authorization.Substring(_bearerStringLength);
            bool isValidToken = _jwtService.ValidateToken(token: jwtToken, VehicleId: vehicleId);
            if (!isValidToken)
            {
                return Unauthorized("Invalid token");
            }
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
        public async Task<IActionResult> GetVehicleSessions([FromHeader(Name = _authString)] string authorization, [FromRoute] int vehicleId)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith(_bearerString))
            {
                return Unauthorized("Authorization header is missing or invalid");
            }
            string jwtToken = authorization.Substring(_bearerStringLength);
            bool isValidToken = _jwtService.ValidateToken(token: jwtToken, VehicleId: vehicleId);
            if (!isValidToken)
            {
                return Unauthorized("Invalid token");
            }
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
