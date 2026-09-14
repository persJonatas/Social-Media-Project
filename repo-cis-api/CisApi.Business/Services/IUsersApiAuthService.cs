using CisApi.Business.DTOs;

namespace CisApi.Business.Services;

/// <summary>
/// Validates JWT tokens by delegating to the UsersAPI.
/// </summary>
public interface IUsersApiAuthService
{
    /// <summary>
    /// Validates a JWT token against the UsersAPI.
    /// Returns the authenticated user if valid, or null if invalid/unreachable.
    /// </summary>
    Task<AuthenticatedUser?> ValidateTokenAsync(string token);
}
