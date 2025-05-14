namespace SlipStream.DTOs.Vehicle;

public record class UpdateVehicleDTO
{
    public required int VehicleId { get; set; }
    public string? NewVehicleName { get; set; }
    public string? NewPassword { get; set; }
    public bool ValidPassword => NewPassword != null && NewPassword.Length >= 8 && NewPassword.Any(char.IsDigit) && NewPassword.Any(char.IsLetter) && NewPassword.Any(char.IsSymbol);
}
