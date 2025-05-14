namespace SlipStream.DTOs.Session;

public record class SendSessionDTO
{
    public int SessionId { get; set; }
    public required string Name { get; set; }
    public required string Location { get; set; }
    public required DateTime CreatedAt { get; set; }
}
