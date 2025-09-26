using app.Services;

public static class GameController
{
    public static void MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        // //遊戲端使用, 玩家驗證
        // app.MapPost("/api/game/verify", (VerifyRequest verifyRequest, GameService service) =>
        // {
        //     var (isValid, userId, errorMsg) = service.VerifyUserToken(verifyRequest);
        //     if (isValid)
        //         return Results.Ok(new { success = true, userId });
        //     else
        //         return Results.BadRequest(new { success = false, message = errorMsg });
        // });

        //遊戲端使用, 玩家下注時, 遊戲端同時打下注跟結算
        app.MapPost("/api/game/playerBetAndSettle", (BetRequest betRequest, GameService service) =>
        {

            var (success, balance, errorMsg) = service.PlayerBetAndSettle(betRequest);

            if (success)
                return Results.Ok(new { success = true, balance });
            else
                return Results.BadRequest(new { success = false, message = errorMsg });
        });
    }

}
