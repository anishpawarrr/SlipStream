using System;
using SlipStream.DTOs;
using SlipStream.DTOs.Session;

namespace SlipStream.Services;

public interface ISession
{
    Task<ReturnDTO> CreateSessionAsync(CreateSessionDTO createSessionDTO);
    Task<ReturnDTO> DeleteSessionAsync(int SessionId, int vehicleId);
    Task<ReturnDTO> GetSessionAsync(int sessionId, int vehicleId);
    Task<ReturnDTO> GetVehicleSessionsAsync(int vehicleId);
}
