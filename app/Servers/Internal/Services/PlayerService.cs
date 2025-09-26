namespace app.Services;

public class PlayerService
{
    private readonly SessionService _sessionService;

    public PlayerService(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public PlayerBetResponse PlaceBet(PlayerBetRequest req)
    {
        var verify = _sessionService.VerifySession(new SessionVerifyRequest(req.SessionToken, req.UserId));
        if (!verify.Valid || verify.Balance < req.BetAmount)
            return new PlayerBetResponse("fail", verify.Balance, string.Empty);

        var newBalance = verify.Balance - req.BetAmount;
        var transactionId = Guid.NewGuid().ToString();

        return new PlayerBetResponse("success", newBalance, transactionId);
    }
}
