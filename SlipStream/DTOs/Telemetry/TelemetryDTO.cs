namespace SlipStream.DTOs.Telemetry;

public record class TelemetryDTO
{
    public required int SessionId { get; set; }
    public long? TimeStamp { get; set; }
    public required List<ValueDTO> Values { get; set; }
}
