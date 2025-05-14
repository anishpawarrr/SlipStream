namespace SlipStream.DTOs.Session;

public record class CreateSessionSuccessDTO
{
    public required int Id { get; set; }
    public required string Jwt { get; set; }
}
