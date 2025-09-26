using app.Services;

public static class SessionController
{
    public static void MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/session/verify", (SessionVerifyRequest req, SessionService service) =>
        {
            var result = service.VerifySession(req);
            return Results.Ok(result);
        });
    }
}
