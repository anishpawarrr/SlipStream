using System;
using System.Runtime.CompilerServices;
using SlipStream.Data;
using SlipStream.DTOs;
using SlipStream.DTOs.Telemetry;

namespace SlipStream.Services;

public interface IConsume
{
    Task<ReturnDTO> ConsumeFromDBAsync(int SessionId, AppDbContext dbContext);
    IAsyncEnumerable<TelemetryDTO> ConsumeFromKafkaAsync(int SessionId, CancellationToken cancellationToken = default);
}
