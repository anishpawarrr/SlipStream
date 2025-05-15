using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SlipStream.Data;
using SlipStream.DTOs.Telemetry;
using SlipStream.Services;

namespace SlipStream.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AcquireController : ControllerBase
    {
        private readonly IProduce _producer;
        private readonly AppDbContext _dbContext;
        private readonly IJWTService _jwtService;
        private const string _authString = "Authorization", _bearerString = "Bearer ";
        private const int _bearerStringLength = 7;
        public AcquireController( [FromServices] IProduce producer, [FromServices] AppDbContext dbContext, [FromServices] IJWTService jwtService)
        {
            _jwtService = jwtService;
            _dbContext = dbContext;
            _producer = producer;
        }

        [HttpPost("", Name = "AcquireTelemetry")]
        public async Task<IActionResult> AcquireTelemetry([FromHeader(Name = _authString)] string authorization , [FromBody] TelemetryDTO telemetryDTO)
        {

            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith(_bearerString))
            {
                return Unauthorized("Authorization header is missing or invalid");
            }
            string jwtToken = authorization.Substring(_bearerStringLength);
            bool isValidToken = _jwtService.ValidateToken(token: jwtToken, VehicleId: telemetryDTO.VehicleId, sessionId: telemetryDTO.SessionId);
            if (!isValidToken)
            {
                return Unauthorized("Invalid token");
            }
            var result = await _producer.ProduceAsync(telemetryDTO.SessionId, telemetryDTO, _dbContext);
            if (result.Status)
            {
                return Ok(result.Message);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("batch", Name = "AcquireTelemetryBatch")]
        public async Task<IActionResult> AcquireTelemetryBatch([FromHeader(Name = _authString)] string authorization , [FromBody] TelemetryBatchDTO telemetryBatchDTO )
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith(_bearerString))
            {
                return Unauthorized("Authorization header is missing or invalid");
            }
            string jwtToken = authorization.Substring(_bearerStringLength);
            bool isValidToken = _jwtService.ValidateToken(token: jwtToken, VehicleId: telemetryBatchDTO.VehicleId, sessionId: telemetryBatchDTO.SessionId);
            if (!isValidToken)
            {
                return Unauthorized("Invalid token");
            }

            var result = await _producer.ProduceAsync(telemetryBatchDTO.SessionId, telemetryBatchDTO, _dbContext);
            if (result.Status)
            {
                return Ok(result.Message);
            }
            return BadRequest(result.Message);
        }

        [HttpGet("topics", Name = "GetTopics")]
        public IActionResult GetTopics()
        {
            return Ok(_producer.GetTopics());
        }

    }
}
