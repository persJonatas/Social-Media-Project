using System.Text.Json.Serialization;

namespace CisApi.Business.DTOs;

/// <summary>
/// Request body sent to UsersAPI POST /api/v1/auth/validate.
/// </summary>
public class UsersApiValidateRequest
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
