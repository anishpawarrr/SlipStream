using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SlipStream.Services;

public class JWTService : IJWTService
{
    private readonly IConfiguration _configuration;
    private readonly byte[] _Key;
    private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
    private readonly string ClaimSessionId = "SessionId", ClaimVehicleId = "VehicleId";


    public JWTService(IConfiguration configuration)
    {
        _configuration = configuration;


        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey)) throw new Exception("Jwt Key is not configured");

        _Key = Encoding.UTF8.GetBytes(jwtKey);
    }

    public string GenerateToken(int VehicleId, int sessionId)
    {

        Claim[] Claims = [
                new Claim(ClaimVehicleId, VehicleId.ToString()),
                new Claim(ClaimSessionId, sessionId.ToString())
            ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(Claims),
            Expires = DateTime.UtcNow.AddHours(3),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_Key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);

    }

    public bool ValidateToken(string token, int VehicleId, int sessionId)
    {
        if (string.IsNullOrEmpty(token)) return false;
        try{
            _tokenHandler.ValidateToken(token, new TokenValidationParameters{
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_Key),
                ValidateLifetime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;

            Claim? vehicleIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimVehicleId);
            Claim? sessionIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimSessionId);

            if (vehicleIdClaim is null || sessionIdClaim is null) return false;

            if (!int.TryParse(vehicleIdClaim.Value, out int parsedVehicleId) || 
                !int.TryParse(sessionIdClaim.Value, out int parsedSessionId))
                return false;

            return VehicleId == parsedVehicleId && sessionId == parsedSessionId;

        }catch{
            return false;
        }
    }
}
