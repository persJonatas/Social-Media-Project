namespace CisApi.Business.DTOs;

/// <summary>
/// Represents an authenticated user extracted from a validated JWT token.
/// </summary>
public record AuthenticatedUser(string UserId, string Login);
