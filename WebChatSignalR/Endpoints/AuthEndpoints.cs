namespace WebChatSignalR.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth");

            group.MapPost("/register", Register);
            group.MapPost("/login", Login);
            group.MapPost("/logout", Logout);

            return app;
        }

        private static IResult Register()
        {
            return Results.Ok("Register endpoint");
        }

        private static IResult Login()
        {
            return Results.Ok("Login endpoint");
        }

        private static IResult Logout()
        {
            return Results.Ok("Logout endpoint");
        }
    }
}
