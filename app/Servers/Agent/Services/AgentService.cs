using MySql.Data.MySqlClient;
using Dapper;
using app.Common;

namespace app.Servers.Agent.Services;

public class AgentService
{
    private readonly AgentDao _agentDao = new AgentDao();

    public string GetAesKeyForAgent(int agentId)
    {
        var info = _agentDao.GetAgentInfo(agentId);
        return info?.AesKey ?? string.Empty;
    }

    public string GetHmacKeyForAgent(int agentId)
    {
        var info = _agentDao.GetAgentInfo(agentId);
        return info?.HmacKey ?? string.Empty;
    }

    public AgentDao.AgentInfoDto? GetAgentInfo(int agentId)
    {
        return _agentDao.GetAgentInfo(agentId);
    }

    // <summary>
    // 玩家登入，成功回傳 遊戲連結
    // </summary>
    public (bool success, string gameUrl, string errorMsg) PlayerLogin(LoginRequest loginRequest)
    {
        Console.WriteLine($"[AgentService] Player login request: {System.Text.Json.JsonSerializer.Serialize(loginRequest)}");
        loginRequest.name = loginRequest.agentId.ToString() + '_' + loginRequest.name.Trim();

        // 1. 更新玩家信息，不存在則創建
        var (result, err) = UpdateAccount(loginRequest.agentId, loginRequest.name);
        if (result != 0)
        {
            return (false, string.Empty, $"更新或創建玩家失敗: {err}");
        }

        // 2. 產生遊戲連結
        string gameUrl = $"https://game.example.com/play?user={loginRequest.name}&lang={loginRequest.lang}&currency={loginRequest.currency}&gameId={loginRequest.gameId}&token=exampletoken";

        return (true, gameUrl, string.Empty);
    }
    public (bool success, decimal balance, string errorMsg) PlayerDeposit(TransferRequest transferRequest)
    {
        Console.WriteLine($"[AgentService] Player deposit request: {System.Text.Json.JsonSerializer.Serialize(transferRequest)}");
        // 檢查錢包type, 單一錢包則略過
        if (transferRequest.walletType == WalletType.Single)
        {
            // 單一錢包, 直接回傳失敗狀態碼
            return (false, 0, "單一錢包不支持此操作");
        }

        // 判斷金額正常與否
        if (transferRequest.amount <= 0)
        {
            return (false, 0, "金額必須大於0");
        }

        transferRequest.name = transferRequest.agentId.ToString() + '_' + transferRequest.name.Trim();

        // redis檢查, 確認該玩家否正在上下分(下一階段再補)
        // orderid檢查, 確認該訂單是否已處理過(下一階段再補)

        // 1. 代理商餘額減少
        var (agentMoneyResult, agentBalanceNew, agentErr) = _agentDao.UpdateAgentBalanceSql(transferRequest.agentId, -transferRequest.amount);
        if (agentMoneyResult != 0)
            return (false, 0, $"代理商餘額更新失敗: {agentErr}");

        Console.WriteLine($"[AgentService] Agent {transferRequest.agentId} balance decreased to {agentBalanceNew}");
        // 2. 玩家餘額增加
        var (playerMoneyResult, playerBalanceNew, playerErr) = _agentDao.UpdatePlayerBalanceSql(transferRequest.name, transferRequest.amount);
        if (playerMoneyResult != 0)
        {
            // 玩家加錢失敗, 需要補回代理商的錢
            var (rollBackResult, _, rollBackErr) = _agentDao.UpdateAgentBalanceSql(transferRequest.agentId, transferRequest.amount);
            if (rollBackResult != 0)
            {
                return (false, 0, $"玩家上分失敗 回滾代理金額失敗: {playerErr}, Rollback agent failed: {rollBackErr}");
            }
            return (false, 0, $"玩家上分失敗 回滾代理金額成功: {playerErr}");
        }

        Console.WriteLine($"[AgentService] Player {transferRequest.name} balance increased to {playerBalanceNew}");
        // 3. 查詢玩家最新餘額（可依需求調整）
        decimal balance = playerBalanceNew;

        return (true, balance, string.Empty);
    }

    public (bool success, decimal balance, string errorMsg) PlayerWithdrawal(TransferRequest transferRequest)
    {
        Console.WriteLine($"[AgentService] Player withdrawal request: {System.Text.Json.JsonSerializer.Serialize(transferRequest)}");
        // 檢查錢包type, 單一錢包則略過
        if (transferRequest.walletType == WalletType.Single)
        {
            // 單一錢包, 直接回傳失敗狀態碼
            return (false, 0, "單一錢包不支持此操作");
        }

        // 判斷金額正常與否
        if (transferRequest.amount <= 0)
        {
            return (false, 0, "金額必須大於0");
        }

        transferRequest.name = transferRequest.agentId.ToString() + '_' + transferRequest.name.Trim();

        // redis檢查, 確認該玩家否正在上下分(下一階段再補)
        // orderid檢查, 確認該訂單是否已處理過(下一階段再補)

        // 1. 玩家餘額減少
        var (playerMoneyResult, playerBalanceNew, playerErr) = _agentDao.UpdatePlayerBalanceSql(transferRequest.name, -transferRequest.amount);
        if (playerMoneyResult != 0)
            return (false, 0, $"玩家下分失敗: {playerErr}");

        Console.WriteLine($"[AgentService] Player {transferRequest.name} balance decreased to {playerBalanceNew}");
        // 2. 代理商餘額增加;
        var (agentMoneyResult, agentBalanceNew, agentErr) = _agentDao.UpdateAgentBalanceSql(transferRequest.agentId, transferRequest.amount);
        if (agentMoneyResult != 0)
        {
            // 代理商加錢失敗, 需要補回玩家的錢
            var (rollBackResult, _, rollBackErr) = _agentDao.UpdatePlayerBalanceSql(transferRequest.name, transferRequest.amount);
            if (rollBackResult != 0)
            {
                return (false, 0, $"代理商下分失敗 回滾玩家金額失敗: {agentErr}, Rollback player failed: {rollBackErr}");
            }
            return (false, 0, $"代理商下分失敗 回滾玩家金額成功: {agentErr}");
        }
        Console.WriteLine($"[AgentService] Agent {transferRequest.agentId} balance increased to {agentBalanceNew}");
        // 3. 查詢玩家最新餘額（可依需求調整）
        decimal balance = playerBalanceNew;
        return (true, balance, string.Empty);
    }

    public (bool success, decimal balance, string errorMsg) GetPlayerBalance(BalanceRequest balanceRequest)
    {
        Console.WriteLine($"[AgentService] Get player balance request: {System.Text.Json.JsonSerializer.Serialize(balanceRequest)}");
        // 檢查錢包type, 單一錢包則略過
        if (balanceRequest.walletType == WalletType.Single)
        {
            // 單一錢包, 直接回傳失敗狀態碼
            return (false, 0, "單一錢包不支持此操作");
        }

        balanceRequest.name = balanceRequest.agentId.ToString() + "_" + balanceRequest.name.Trim();
        
        // 1. 查詢玩家餘額  
        var (Result, playerBalance, Err) = _agentDao.GetPlayerBalance(balanceRequest.name);
        if (Result != 0)
            return (false, 0, $"玩家查詢餘額失敗: {Err}");
        return (true, playerBalance, string.Empty);
    }

    public (int result, string err) UpdateAccount(int agentId, string playerName)
    {
        try
        {
            // 1. 更新玩家最後登入時間, 失敗則創建
            var (Result, Err) = _agentDao.UpdateAccount(agentId, playerName);
            if (Result == -1)
            {
                return (1, $"更新玩家最後登入時間失敗: {Err}");
            }
            else if (Result == 0)
            {
                // 2. 創建新玩家
                _agentDao.CreatePlayer(agentId, playerName);
                Console.WriteLine($"[AgentService] Created new player: {playerName} for agent {agentId}");
            }

            return (0, string.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AgentService] Error in UpdateAccount transaction: {ex.Message}");
            return (1, "Database error during account update");
        }
    }
}