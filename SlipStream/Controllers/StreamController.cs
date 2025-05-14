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
        public StreamController( [FromServices] IConsume consumer, [FromServices] AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _consumer = consumer;

        }

        [HttpGet("live/{sessionId}", Name = "StreamSessionData")]
        public async IAsyncEnumerable<TelemetryDTO> GetStreamSessionData( [FromRoute] int sessionId, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var telemetry in _consumer.ConsumeFromKafkaAsync(sessionId, cancellationToken)){
                yield return telemetry;
            }
        }

        [HttpGet("db/{sessionId}", Name = "SessionDataFromDB")]
        public async Task<IActionResult> GetSessionDataFromDB( [FromRoute] int sessionId)
        {
            var result = await _consumer.ConsumeFromDBAsync(sessionId, _dbContext);
            if (result.Status)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }
    }
}
