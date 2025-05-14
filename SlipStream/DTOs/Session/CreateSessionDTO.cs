namespace SlipStream.DTOs.Session;

public record class CreateSessionDTO
{
    public required int VehicleId { get; set; }
    public required string Name { get; set; }
    public required string Location { get; set; }
    public bool VehicleTime { get; set; } = true;
    // public DateTime? Time { get; set; } = DateTime.UtcNow;
}