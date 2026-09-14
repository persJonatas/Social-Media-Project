using System.Net;
using System.Text.Json;
using CisApi.Business.Services;

namespace CisApi.Presentation.Middleware;

public class UsersApiAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UsersApiAuthMiddleware> _logger;

    public UsersApiAuthMiddleware(RequestDelegate next, ILogger<UsersApiAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUsersApiAuthService authService)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        _logger.LogInformation("Middleware - Path: {Path}", path);

        if (path.StartsWith("/health") ||
            path.StartsWith("/swagger") ||
            path.StartsWith("/scalar") ||
            path.StartsWith("/openapi") ||
            path.StartsWith("/v3/api-docs"))
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.ToString();

        _logger.LogInformation("Middleware - AuthHeader: {Auth}", authHeader);

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            await RespondUnauthorizedAsync(context, "Unauthorized");
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        _logger.LogInformation("Middleware - Token: {Token}", token);
        
        var user = await authService.ValidateTokenAsync(token);

        _logger.LogInformation("Middleware - User: {User}", user?.UserId ?? "null");

        if (user is null)
        {
            await RespondUnauthorizedAsync(context, "Unauthorized");
            return;
        }

        context.Items["UserId"] = user.UserId;
        context.Items["UserLogin"] = user.Login;

        await _next(context);
    }

    private static async Task RespondUnauthorizedAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            error = message,
            timestamp = DateTime.UtcNow.ToString("O")
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}