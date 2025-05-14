namespace SlipStream.DTOs.Vehicle;

public record class RetrieveVehicleDTO
{
    public required string Name { get; set; }
    public required string Password { get; set; }
}
