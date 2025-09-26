using app.Common;

namespace app.Services;

public class GameService
{
    // 模擬生成遊戲連結 (真實環境會呼叫遊戲供應商 API)
    public string GetGameUrl(int userId, string gameCode)
    {
        // TODO: 驗證 userId 是否存在、權限是否正確
        var sessionToken = Guid.NewGuid().ToString("N"); // 產生一個隨機 session
        var gameUrl = $"https://game-provider.com/play?token={sessionToken}&game={gameCode}&user={userId}";
        return gameUrl;
    }

    // 驗證遊戲端傳來的 userToken
    // public (bool isValid, int userId, string errorMsg) VerifyUserToken(VerifyRequest verifyRequest)
    // {
    // }

    // 玩家下注並結算
    public (bool success, decimal balance, string errorMsg) PlayerBetAndSettle(BetRequest betRequest)
    {
        try
        { 
            // log 
            // 分成單一錢包跟轉帳錢包
            // 單一錢包 , 會通知代理商進行扣款
            // 轉帳錢包, 直接在錢包端扣款
            if (betRequest.WalletType == WalletType.Single)
            {
                // 單一錢包, 會通知代理商進行扣款(下一階段再補)
                return (false, 0, "單一錢包不支持此操作");
            }
            else
            {
                // 轉帳錢包, 直接在錢包端扣款(下一階段再補)
                return (false, 0, "轉帳錢包不支持此操作");
            }

        
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameService] Error in PlayerBetAndSettle: {ex.Message}");
            return (false, 0, "Internal server error");
        }

    }
}
