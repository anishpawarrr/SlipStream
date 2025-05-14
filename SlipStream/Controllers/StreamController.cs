using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using SlipStream.Data;
using SlipStream.DTOs.Telemetry;
using SlipStream.Services;

namespace SlipStream.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        private readonly IConsume _consumer;
        private readonly AppDbContext _dbContext;
        private readonly IJWTService _jwtService;
        private const string _authString = "Authorization", _bearerString = "Bearer ";
        private const int _bearerStringLength = 7;
        public StreamController( [FromServices] IConsume consumer, [FromServices] AppDbContext dbContext, IJWTService jwtService)
        {
            _dbContext = dbContext;
            _consumer = consumer;
            _jwtService = jwtService;

        }

        [HttpGet("live/{vehicleId}/{sessionId}", Name = "StreamSessionData")]
        public async IAsyncEnumerable<SendTelemetryDTO> GetStreamSessionData( [FromRoute] int sessionId, [FromRoute] int vehicleId , [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith(_bearerString))
            // {
            //     throw new UnauthorizedAccessException("Authorization header is missing or invalid");
            // }

            // string jwtToken = authorization.Substring(_bearerStringLength);
            // bool isValidToken = _jwtService.ValidateToken(token: jwtToken, VehicleId: vehicleId, sessionId: sessionId);
            // if (!isValidToken)
            // {
            //     throw new UnauthorizedAccessException("Invalid token");
            // }

            await foreach (var telemetry in _consumer.ConsumeFromKafkaAsync(sessionId, cancellationToken)){
                yield return telemetry;
            }
        }

        [HttpGet("db/{vehicleId}/{sessionId}", Name = "SessionDataFromDB")]
        public async Task<IActionResult> GetSessionDataFromDB( [FromHeader(Name = _authString)] string authorization , [FromRoute] int sessionId, [FromRoute] int vehicleId)
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
            var result = await _consumer.ConsumeFromDBAsync(vehicleId, sessionId, _dbContext);
            if (result.Status)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }
    }
}
