using WebChatSignalR.Endpoints;
using WebChatSignalR.Hubs;
using WebChatSignalR.Security;

var builder = WebApplication.CreateBuilder(args);

// SignalR försöker använda WebSockets först och kan automatiskt falla tillbaka
// till andra transportprotokoll om WebSockets inte fungerar.
builder.Services.AddSignalR();

// ChatRateLimiter sparar senaste meddelandetid per anslutning.
// Den registreras som singleton så samma instans används av alla hubb-anrop.
builder.Services.AddSingleton<ChatRateLimiter>();



// Cookie Authentication skapar en riktig inloggad användare i ASP.NET Core.
// När användaren loggar in skapar servern en signerad auth-cookie med claims.
//Vid senare requests fyller ASP.NET Core
// HttpContext.User, vilket gör att RequireAuthorization och [Authorize] fungerar.

builder.Services
    .AddAuthentication("ChatCookie")
    .AddCookie("ChatCookie", options =>
    {
        options.Cookie.Name = ChatUserCookie.CookieName;

        // HttpOnly skyddar cookien från att läsas direkt av JavaScript.
        options.Cookie.HttpOnly = true;

        // SecurePolicy.Always gör att cookien bara skickas över HTTPS.
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;

        options.LoginPath = "/login";


        // API:et ska returnera 401 i stället för att försöka redirecta till en login-sida.
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

// Redirectar HTTP till HTTPS.
// När appen körs med certifikat används TLS, vilket krypterar trafiken
// mellan klient och server.
app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// Authentication måste köras före Authorization.
// Annars vet inte ASP.NET Core vem användaren är när endpoints/hubbar skyddas.
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();

// Hubben skyddas med RequireAuthorization så endast användare som loggat in
// och fått en giltig auth-cookie kan ansluta till chatten.
app.MapHub<ChatHub>("/hubs/chat").RequireAuthorization();

app.Run();
