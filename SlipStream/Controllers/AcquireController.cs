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
        public AcquireController( [FromServices] IProduce producer, [FromServices] AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _producer = producer;
        }

        [HttpPost("{sessionId}", Name = "AcquireTelemetry")]
        public async Task<IActionResult> AcquireTelemetry( [FromRoute] int sessionId, [FromBody] TelemetryDTO telemetryDTO)
        {
            var result = await _producer.ProduceAsync(sessionId, telemetryDTO, _dbContext);
            if (result.Status)
            {
                return Ok(result.Message);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("batch/{sessionId}", Name = "AcquireTelemetryBatch")]
        public async Task<IActionResult> AcquireTelemetryBatch([FromRoute] int sessionId, [FromBody] TelemetryBatchDTO telemetryBatchDTO)
        {
            var result = await _producer.ProduceAsync(sessionId, telemetryBatchDTO, _dbContext);
            if (result.Status)
            {
                return Ok(result.Message);
            }
            return BadRequest(result.Message);
        }

    }
}
