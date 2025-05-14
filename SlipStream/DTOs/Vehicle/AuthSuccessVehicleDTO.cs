namespace SlipStream.DTOs.Vehicle;

public record class AuthSuccessVehicleDTO
{
    public required string Jwt { get; set; }
    public required int VehicleId { get; set; }
    public int SessionId { get; set; } = -1;
}