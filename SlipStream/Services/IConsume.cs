using System;
using System.Runtime.CompilerServices;
using SlipStream.Data;
using SlipStream.DTOs;
using SlipStream.DTOs.Telemetry;

namespace SlipStream.Services;

public interface IConsume
{
    Task<ReturnDTO> ConsumeFromDBAsync(int vehicleId, int SessionId, AppDbContext dbContext);
    IAsyncEnumerable<SendTelemetryDTO> ConsumeFromKafkaAsync(int SessionId, CancellationToken cancellationToken = default);
}
