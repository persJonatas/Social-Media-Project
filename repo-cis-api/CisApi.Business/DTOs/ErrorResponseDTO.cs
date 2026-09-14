namespace CisApi.Business.DTOs;

public class ErrorResponseDTO
{
    public string Error { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}