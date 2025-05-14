namespace SlipStream.DTOs.Telemetry;

public record class TelemetryBatchDTO
{
    public required int VehicleId { get; set; }
    public required int SessionId { get; set; }
    public List<BatchValueDTO> Values { get; set; } = [];
}
