using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SlipStream.Data;
using SlipStream.DTOs.Vehicle;
using SlipStream.Services;

namespace SlipStream.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IJWTService _jwtService;
        private readonly IVehicle _vehicleService;
        private readonly AppDbContext _dbContext;
        public VehicleController( [FromServices] IJWTService jwtService, [FromServices] IVehicle vehicleService, [FromServices] AppDbContext dbContext)
        {
            _jwtService = jwtService;
            _vehicleService = vehicleService;
            _dbContext = dbContext;
        }

        [HttpPost("Register", Name = "AddVehicle")]
        public async Task<IActionResult> AddVehicle([FromBody] CreateVehicleDTO createVehicleDTO)
        {
            var result = await _vehicleService.AddVehicleAsync(createVehicleDTO);
            if (result.Status)
            {
                return CreatedAtRoute("Login", result.Data);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("Login", Name = "Login")]
        public async Task<IActionResult> Login([FromBody] RetrieveVehicleDTO retrieveVehicleDTO)
        {
            var result = await _vehicleService.RetrieveVehicleAsync(retrieveVehicleDTO);
            if (result.Status)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        [HttpPut("", Name = "UpdateVehicle")]
        public async Task<IActionResult> UpdateVehicle([FromBody] UpdateVehicleDTO updateVehicleDTO)
        {
            var result = await _vehicleService.UpdateVehicleAsync(updateVehicleDTO);
            if (result.Status)
            {
                return NoContent();
            }
            return BadRequest(result.Message);
        }

        [HttpDelete("", Name = "DeleteVehicle")]
        public async Task<IActionResult> DeleteVehicle([FromBody] DeleteVehicleDTO deleteVehicleDTO)
        {
            var result = await _vehicleService.DeleteVehicleAsync(deleteVehicleDTO);
            if (result.Status)
            {
                return NoContent();
            }
            return BadRequest(result.Message);
        }

    }
}
