using System;

namespace SlipStream.Services;

public interface IJWTService
{
    string GenerateToken(int VehicleId, int sessionId);
    bool ValidateToken(string token, int VehicleId, int sessionId);
}
