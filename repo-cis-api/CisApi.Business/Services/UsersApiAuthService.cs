using System.Net.Http.Json;
using CisApi.Business.DTOs;
using Microsoft.Extensions.Logging;

namespace CisApi.Business.Services;

/// <summary>
/// Validates JWT tokens by calling the UsersAPI POST /api/v1/auth/validate endpoint.
/// </summary>
public class UsersApiAuthService : IUsersApiAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersApiAuthService> _logger;

    public UsersApiAuthService(HttpClient httpClient, ILogger<UsersApiAuthService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AuthenticatedUser?> ValidateTokenAsync(string token)
    {
        try
        {
            var request = new UsersApiValidateRequest { Token = token };
        
            // Log temporário
            _logger.LogInformation("Calling UsersAPI validate with token: {Token}", token);
            _logger.LogInformation("BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
        
            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/validate", request);

            // Log temporário
            _logger.LogInformation("UsersAPI response: {StatusCode}", response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("UsersAPI body: {Body}", responseBody);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<UsersApiValidateResponse>(
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result is null || string.IsNullOrEmpty(result.UserId))
                return null;

            return new AuthenticatedUser(result.UserId, result.Login);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to communicate with UsersAPI.");
            return null;
        }
    }
}
