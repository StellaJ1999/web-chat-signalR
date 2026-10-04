using Microsoft.AspNetCore.Authentication;
using WebChatSignalR.Security;
using WebChatSignalR.Dtos;

namespace WebChatSignalR.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/login", (Delegate)Login);

        //Kräver att användaren är inloggad
        app.MapPost("/logout", (Delegate)Logout).RequireAuthorization();
        app.MapGet("/me", (Delegate)GetCurrentUser).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> Login(LoginRequest request, HttpContext httpContext)
    {
        var validationError = UsernameValidator.Validate(request.Name);

        if (validationError is not null)
        {
            return Results.BadRequest(new { error = validationError });
        }

        var username = UsernameValidator.Clean(request.Name!);
        var principal = ChatUserCookie.CreatePrincipal(username);

        // SignInAsync skapar den signerade auth-cookien.
        // Efter detta räknas användaren som autentiserad av ASP.NET Core.
        await httpContext.SignInAsync(
            ChatUserCookie.AuthScheme,
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
        // Tar bort auth-cookien så användaren inte längre är inloggad.
        await httpContext.SignOutAsync(ChatUserCookie.AuthScheme);

        return Results.Ok(new { message = "Du är utloggad." });
    }

    private static IResult GetCurrentUser(HttpContext httpContext)
    {
        var username = httpContext.User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(username))
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { name = username });
    }
}
