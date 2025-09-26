using app.Common;

// 遊戲端內部打的, req包含traceID, body 包含agentId, playerId, gameCode, betAmount, winAmount, balance
// 回傳 success, balance, errorMsg 
public class BetRequest
{
    public string TraceID { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string GameNo { get; set; } = string.Empty;
    public int GameId { get; set; } = 0;
    public int RoomId { get; set; } = 0;
    public WalletType WalletType { get; set; } = WalletType.Single;
    public string Currency { get; set; } = "TWD";
    public decimal BetAmount { get; set; } = 0;
    public decimal WinAmount { get; set; } = 0;
    public decimal Balance { get; set; } = 0;
}