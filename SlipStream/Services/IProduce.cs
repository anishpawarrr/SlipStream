using System;
using SlipStream.Data;
using SlipStream.DTOs;
using SlipStream.DTOs.Telemetry;

namespace SlipStream.Services;

public interface IProduce
{
    Task<ReturnDTO> ProduceAsync(int SessionId, TelemetryDTO telemetryDTO, AppDbContext dbContext);
    Task<ReturnDTO> ProduceAsync(int SessionId, TelemetryBatchDTO telemetryBatchDTO, AppDbContext dbContext);

    Task<ReturnDTO> InsertAsync(int SessionId, TelemetryDTO telemetryDTO, AppDbContext dbContext);
    Task<ReturnDTO> InsertAsync(int SessionId, TelemetryBatchDTO telemetryBatchDTO, AppDbContext dbContext);

    Task<ReturnDTO> PushOnQueueAsync(int SessionId, TelemetryDTO telemetryDTO);
    Task<ReturnDTO> PushOnQueueAsync(int SessionId, TelemetryBatchDTO telemetryBatchDTO);
}
