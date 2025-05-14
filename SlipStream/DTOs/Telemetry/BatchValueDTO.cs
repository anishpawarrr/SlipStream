namespace SlipStream.DTOs.Telemetry;

public record class BatchValueDTO
{
    public long? TimeStamp { get; set; }
    public List<ValueDTO> BatchValues { get; set; } = [];
}
