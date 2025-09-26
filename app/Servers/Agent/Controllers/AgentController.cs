
using app.Servers.Agent.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public static class AgentController
{
    public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
    {
        // 玩家登入
        app.MapPost("/api/player/login", async (HttpRequest request, AgentService agentService) =>
        {
            // 共通安全檢查與解密
            (bool isValid, IResult errorResult, LoginRequest? loginReq) = await RequestHelper.ValidateAndDecryptRequest<LoginRequest>(request, agentService);
            if (!isValid)
                return errorResult;

            if (loginReq == null)
                return Results.BadRequest("Invalid request");

            var (success, gameUrl, errorMsg) = agentService.PlayerLogin(loginReq);
            if (success)
                return Results.Ok(new { success = true, gameUrl });
            else
                return Results.BadRequest(new { success = false, message = errorMsg });
        });
        
        // 玩家上分
        app.MapPost("/api/player/deposit", async (HttpRequest request, AgentService agentService) =>
        {
            // 共通安全檢查與解密
            (bool isValid, IResult errorResult, TransferRequest? depositReq) = await RequestHelper.ValidateAndDecryptRequest<TransferRequest>(request, agentService);
            if (!isValid)
                return errorResult;

            if (depositReq == null)
                return Results.BadRequest("Invalid request");

            var (success, balance, errorMsg) = agentService.PlayerDeposit(depositReq);
            if (success)
                return Results.Ok(new { success = true, balance });
            else
                return Results.BadRequest(new { success = false, message = errorMsg });
        });

        // 玩家下分
        app.MapPost("/api/player/credit", async (HttpRequest request, AgentService agentService) =>
        {
            // 共通安全檢查與解密
            (bool isValid, IResult errorResult, TransferRequest? depositReq) = await RequestHelper.ValidateAndDecryptRequest<TransferRequest>(request, agentService);
            if (!isValid)
                return errorResult;
            if (depositReq == null)
                return Results.BadRequest("Invalid request");

            var (success, balance, errorMsg) = agentService.PlayerWithdrawal(depositReq);
            if (success)
                return Results.Ok(new { success = true, balance });
            else
                return Results.BadRequest(new { success = false, message = errorMsg });
        });

        // 玩家餘額查詢
        app.MapPost("/api/player/balance", async (HttpRequest request, AgentService agentService) =>
        {
            // 共通安全檢查與解密
            (bool isValid, IResult errorResult, BalanceRequest? balanceReq) = await RequestHelper.ValidateAndDecryptRequest<BalanceRequest>(request, agentService);
            if (!isValid)
                return errorResult;
            if (balanceReq == null)
                return Results.BadRequest("Invalid request");

            var (success, balance, errorMsg) = agentService.GetPlayerBalance(balanceReq);
            if (success)
                return Results.Ok(new { success = true, balance });
            else
                return Results.BadRequest(new { success = false, message = errorMsg });
        });
    }
}
