using System;

namespace SlipStream.Entities;

public class Telemetry
{
    public int Id { get; set; }
    public required long TimeStamp { get; set; }
    public required string Parameter { get; set; }
    public required float State { get; set; }
    public required int SessionId { get; set; }
    public Session? Session { get; set; }
}
