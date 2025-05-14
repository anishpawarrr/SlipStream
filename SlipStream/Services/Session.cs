using System;
using Microsoft.EntityFrameworkCore;
using SlipStream.Data;
using SlipStream.DTOs;
using SlipStream.DTOs.Session;

namespace SlipStream.Services;

public class Session : ISession
{
    private readonly AppDbContext _dbContext;
    private readonly IJWTService _jwtService;

    public Session(AppDbContext dbContext, IJWTService jwtService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
    }

    public async Task<ReturnDTO> CreateSessionAsync(CreateSessionDTO createSessionDTO)
    {

        var session = new Entities.Session
        {
            Name = createSessionDTO.Name,
            Location = createSessionDTO.Location,
            CreatedAt = DateTime.UtcNow,
            FollowsUnixTime = !createSessionDTO.VehicleTime,
            VehicleId = createSessionDTO.VehicleId
        };

        await _dbContext.Sessions.AddAsync(session);
        await _dbContext.SaveChangesAsync();

        return new ReturnDTO
        {
            Status = true,
            Data = new CreateSessionSuccessDTO{
                Id = session.Id,
                Jwt = _jwtService.GenerateToken( sessionId: session.Id, VehicleId: createSessionDTO.VehicleId )
            }
        };
    }

    public async Task<ReturnDTO> DeleteSessionAsync(int SessionId, int VehicleId)
    {
        Entities.Session? session = await SessionExistsAsync(sessionId: SessionId, vehicleId: VehicleId);
        if (session == null)
        {
            return new ReturnDTO
            {
                Message = "Session not found",
                StatusCode = 404
            };
        }

        _dbContext.Sessions.Remove(session);
        await _dbContext.SaveChangesAsync();

        return new ReturnDTO
        {
            Status = true,
            Message = "Session deleted successfully",
            StatusCode = 200
        };
    }

    public async Task<ReturnDTO> GetSessionAsync(int sessionId, int VehicleId)
    {
        Entities.Session? session = await SessionExistsAsync(sessionId: sessionId, vehicleId: VehicleId);
        if (session == null)
        {
            return new ReturnDTO
            {
                Message = "Session not found",
                StatusCode = 404
            };
        }

        return new ReturnDTO{
            Status = true,
            Data = new SendSessionDTO
            {
                SessionId = session.Id,
                Name = session.Name,
                Location = session.Location,
                CreatedAt = session.CreatedAt
            },
            Message = "Session found",
            StatusCode = 200
        };
    }

    public async Task<ReturnDTO> GetVehicleSessionsAsync(int vehicleId)
    {
        List<SendSessionDTO> allSessions = await _dbContext.Sessions
            .Where(s => s.VehicleId == vehicleId)
            .Select(s => new SendSessionDTO
            {
                SessionId = s.Id,
                Name = s.Name,
                Location = s.Location,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return new ReturnDTO
        {
            Status = true,
            Data = allSessions,
            Message = "Sessions found",
            StatusCode = 200
        };
    }

    private async Task<Entities.Session?> SessionExistsAsync(int sessionId, int vehicleId)
    {
        return await _dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.VehicleId == vehicleId);
    }
}