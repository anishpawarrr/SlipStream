using System;

namespace SlipStream.Entities;

public class Session
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Location { get; set; }
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool FollowsUnixTime { get; set; } = false;
    public required int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}
