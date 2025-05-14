using SlipStream.Data;
using SlipStream.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;

namespace SlipStream.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IJWTService _jwtService;

        public IdentityController([FromServices] AppDbContext dbContext, [FromServices] IJWTService jwtService){
            _dbContext = dbContext;
            _jwtService = jwtService;
        }

        [HttpGet("Generate/{id}/{sessionId}")]
        public IActionResult Generatetoken( [FromRoute] int id, [FromRoute] int sessionId){
            string token = _jwtService.GenerateToken(VehicleId: id, sessionId: sessionId);
            return Ok(token);
        }

        [HttpGet("Validate/{id}/{sessionId}/{token}")]
        public IActionResult Validatetoken( [FromRoute] int id, [FromRoute] int sessionId,[FromRoute] string token){
            bool valid = _jwtService.ValidateToken(token: token, VehicleId: id, sessionId: sessionId);
            
            if (valid) return NoContent();
            return Unauthorized("Invalid token");
        }
    }
}
