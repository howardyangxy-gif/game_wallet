using app.Services;

public static class PlayerController
{
    public static void MapPlayerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/player/bet", (PlayerBetRequest req, PlayerService service) =>
        {
            var result = service.PlaceBet(req);
            return Results.Ok(result);
        });
    }
}
