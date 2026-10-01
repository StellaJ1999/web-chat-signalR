using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace WebChatSignalR.Endpoints;

public static class AuthEndpoints
{
    private const string AuthScheme = "ChatCookie";
    private const int MaxUsernameLength = 20;

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/login", Login);
        app.MapPost("/logout", Logout).RequireAuthorization();
        app.MapGet("/me", GetCurrentUser).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> Login(LoginRequest request, HttpContext httpContext)
    {
        var username = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            return Results.BadRequest(new { error = "Användarnamn krävs." });
        }

        if (username.Length > MaxUsernameLength)
        {
            return Results.BadRequest(new
            {
                error = $"Användarnamnet får max vara {MaxUsernameLength} tecken."
            });
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(claims, AuthScheme);
        var principal = new ClaimsPrincipal(identity);

                await httpContext.SignInAsync(
            AuthScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            }
        );

        return Results.Ok(new { name = username });
    }
    private static async Task<IResult> Logout(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(AuthScheme);

        return Results.Ok(new { message = "Du är utloggad." });
    }

    private static IResult GetCurrentUser(HttpContext httpContext)
    {
        var username = httpContext.User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(username))
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            name = username,
            isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false
        });
    }
}

public sealed class LoginRequest
{
    public string? Name { get; set; }
}