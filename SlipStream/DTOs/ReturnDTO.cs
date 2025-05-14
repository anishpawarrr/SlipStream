namespace SlipStream.DTOs;

public record class ReturnDTO
{
    public bool Status { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; } = null;
    public int StatusCode { get; set; } = 500;
}
