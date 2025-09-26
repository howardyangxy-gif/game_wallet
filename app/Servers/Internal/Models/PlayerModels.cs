public record PlayerBetRequest(string SessionToken, int UserId, decimal BetAmount, string RoundId);
public record PlayerBetResponse(string Status, decimal Balance, string TransactionId);
