using System.Text.Json.Serialization;

namespace CisApi.Business.DTOs;

/// <summary>
/// Response body from UsersAPI POST /api/v1/auth/validate.
/// </summary>
public class UsersApiValidateResponse
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;
}
