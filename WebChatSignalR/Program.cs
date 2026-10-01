using WebChatSignalR.Endpoints;
using WebChatSignalR.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

// När användaren loggar in skapar servern en signerad auth-cookie
// som innehåller användarens claims, till exempel användarnamn.
// Vid varje senare request läser ASP.NET Core cookien och fyller
// HttpContext.User, vilket gör att endpoints och SignalR-hubbar kan
// använda RequireAuthorization(), [Authorize] och Context.User.
// HttpOnly gör att JavaScript inte kan läsa cookien.
// SecurePolicy.Always gör att cookien bara skickas över HTTPS.

builder.Services
    .AddAuthentication("ChatCookie")
    .AddCookie("ChatCookie", options =>
    {
        options.Cookie.Name = "chat_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;

        options.LoginPath = "/login";

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapHub<ChatHub>("/hubs/chat").RequireAuthorization();

app.Run();
