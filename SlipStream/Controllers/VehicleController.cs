using Microsoft.AspNetCore.Authorization;
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
        private const string _authString = "Authorization", _bearerString = "Bearer ";
        private const int _bearerStringLength = 7;
        // private readonly AppDbContext _dbContext;
        public VehicleController( [FromServices] IJWTService jwtService, [FromServices] IVehicle vehicleService )
        {
            _jwtService = jwtService;
            _vehicleService = vehicleService;
            // _dbContext = dbContext;
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
        public async Task<IActionResult> UpdateVehicle( [FromBody] UpdateVehicleDTO updateVehicleDTO, [FromHeader(Name = _authString)] string authorization )
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith(_authString))
            {
                return Unauthorized("Authorization header is missing or invalid");
            }

            string jwtToken = authorization.Substring(_bearerStringLength);

            bool isValidToken = _jwtService.ValidateToken(token: jwtToken, VehicleId: updateVehicleDTO.VehicleId);
            if (!isValidToken)
            {
                return Unauthorized("Invalid token");
            }

            var result = await _vehicleService.UpdateVehicleAsync(updateVehicleDTO);
            if (result.Status)
            {
                return NoContent();
            }
            return BadRequest(result.Message);
        }

        [HttpDelete("", Name = "DeleteVehicle")]
        public async Task<IActionResult> DeleteVehicle([FromBody] DeleteVehicleDTO deleteVehicleDTO, [FromHeader(Name = _authString)] string authorization )
        {

            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith(_bearerString))
            {
                return Unauthorized("Authorization header is missing or invalid");
            }
            string jwtToken = authorization.Substring(_bearerStringLength);
            bool isValidToken = _jwtService.ValidateToken(token: jwtToken, VehicleId: deleteVehicleDTO.Id);
            if (!isValidToken)
            {
                return Unauthorized("Invalid token");
            }

            var result = await _vehicleService.DeleteVehicleAsync(deleteVehicleDTO);
            if (result.Status)
            {
                return NoContent();
            }
            return BadRequest(result.Message);
        }

    }
}
