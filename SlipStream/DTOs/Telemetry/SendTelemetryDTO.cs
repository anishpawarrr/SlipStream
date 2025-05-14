namespace SlipStream.DTOs.Telemetry;

public record class SendTelemetryDTO
{
    public long? TimeStamp { get; set; }
    public required List<ValueDTO> Values { get; set; }
}
