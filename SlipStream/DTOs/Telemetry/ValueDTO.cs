namespace SlipStream.DTOs.Telemetry;

public record class ValueDTO
{
    public required string Parameter { get; set; }
    public float State { get; set; } = 0;
}
