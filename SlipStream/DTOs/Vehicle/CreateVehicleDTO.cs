namespace SlipStream.DTOs.Vehicle;

public record class CreateVehicleDTO
{
    public required string Name { get; set; }
    public required string Password { get; set; }
    public bool ValidPassword => Password.Length >= 8 && Password.Any(char.IsDigit) && Password.Any(char.IsLetter) && Password.Any(char.IsSymbol);
}