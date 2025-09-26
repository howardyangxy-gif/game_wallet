using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using app.Servers.Agent.Services;
using app.Common;

public static class RequestHelper
{
    public static async Task<(bool isValid, IResult errorResult, T? data)> ValidateAndDecryptRequest<T>(HttpRequest request, AgentService agentService) where T : AgentRequestBase
    {
        // // 強制 HTTPS
        // if (!request.IsHttps)
        //     return (false, Results.BadRequest("HTTPS required"), default);

        int agentId = request.Headers.TryGetValue("X-Agent-Id", out var agentIdVals) ? int.Parse(agentIdVals.ToString()) : 0;
        if (agentId == 0)
            return (false, Results.BadRequest("Missing agentId"), default);

        // 從db取得agent
        var agentInfo = agentService.GetAgentInfo(agentId);
        Console.WriteLine($"[AgentInfo] {System.Text.Json.JsonSerializer.Serialize(agentInfo)}");

        if (agentInfo == null)
            return (false, Results.BadRequest("Invalid agentId"), default);
        if (agentInfo.Status != 0)
            return (false, Results.BadRequest("Agent inactive"), default);

        // 驗證 IP 是否在白名單
        // var ip = GetRequestIp(request);
        // if (!string.IsNullOrEmpty(agentInfo.whiteIp))
        // {
        //     var whiteIpList = agentInfo.whiteIp.Split(',').Select(x => x.Trim()).ToList();
        //     if (ip == null || !whiteIpList.Contains(ip))
        //         return (false, Results.BadRequest($"IP {ip} not allowed"), default);
        // }

        string? aesKey = agentInfo!.AesKey;
        string? hmacKey = agentInfo!.HmacKey;

        if (string.IsNullOrEmpty(aesKey) || string.IsNullOrEmpty(hmacKey))
            return (false, Results.BadRequest("Invalid agentId"), default);

        string timestamp = request.Headers.TryGetValue("X-Timestamp", out var tsVals) ? tsVals.ToString() : "";
        string nonce = request.Headers.TryGetValue("X-Nonce", out var nonceVals) ? nonceVals.ToString() : "";
        string signature = request.Headers.TryGetValue("X-Signature", out var sigVals) ? sigVals.ToString() : "";
        if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(nonce) || string.IsNullOrEmpty(signature))
            return (false, Results.BadRequest("Missing security headers"), default);

        using var reader = new StreamReader(request.Body);
        var encryptedBase64 = await reader.ReadToEndAsync();
        if (string.IsNullOrEmpty(encryptedBase64))
            return (false, Results.BadRequest("Empty body"), default);

        var signRaw = agentId + timestamp + nonce + encryptedBase64;
        Console.WriteLine($"[SignRaw] {signRaw}");
        var valid = VerifyHmacSignature(signRaw, signature, hmacKey);
        if (!valid)
            return (false, Results.BadRequest("Signature invalid"), default);

        string json;
        try
        {
            json = DecryptAesBase64(encryptedBase64, aesKey);
            Console.WriteLine($"[DecryptedBody] {json}");
        }
        catch
        {
            return (false, Results.BadRequest("Decrypt failed"), default);
        }

        var data = System.Text.Json.JsonSerializer.Deserialize<T>(json);

        if (data == null)
            return (false, Results.BadRequest("Invalid data"), default);

        // 檢查 timestamp/nonce 是否過期或重放（防止重放攻擊）
        if (!long.TryParse(timestamp, out var tsMillis))
            return (false, Results.BadRequest("Invalid timestamp"), default);
        var ts = DateTimeOffset.FromUnixTimeMilliseconds(tsMillis).UtcDateTime;
        var now = DateTime.UtcNow;
        if (Math.Abs((now - ts).TotalMinutes) > 5)
            return (false, Results.BadRequest("Timestamp expired"), default);

        if (!RequestNonceCache.TryAdd(nonce, now))
            return (false, Results.BadRequest("Nonce reused"), default);
        RequestNonceCache.RemoveOld(now.AddMinutes(-10));

        // 補上 agentId
        data.agentId = agentId;
        // 補上 walletType
        data.walletType = (WalletType)agentInfo.WalletType;
        
        return (true, Results.Ok(), data);
    }

        public static bool VerifyHmacSignature(string raw, string signature, string key)
    {
        // HMACSHA256 驗證，簽章為 hex 字串
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(raw));
        var hashHex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        return string.Equals(hashHex, signature, StringComparison.OrdinalIgnoreCase);
    }

    public static string DecryptAesBase64(string encryptedBase64, string aesKey)
    {
        // AES-128-ECB, PKCS7 padding, base64 輸入
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.KeySize = 128;
        aes.Mode = System.Security.Cryptography.CipherMode.ECB;
        aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
        aes.Key = System.Text.Encoding.UTF8.GetBytes(aesKey);
        // ECB 不用 IV
        var encryptedBytes = Convert.FromBase64String(encryptedBase64);
        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return System.Text.Encoding.UTF8.GetString(decryptedBytes);
    }

    public static string? GetRequestIp(HttpRequest request)
    {
        if (request.Headers.TryGetValue("x-real-ip", out var realIp) && !string.IsNullOrWhiteSpace(realIp))
        {
            return realIp.ToString();
        }
        else if (request.Headers.TryGetValue("x-forwarded-for", out var fwdIp) && !string.IsNullOrWhiteSpace(fwdIp))
        {
            return fwdIp.ToString().Split(',')[0].Trim();
        }
        else
        {
            var remoteIp = request.HttpContext.Connection.RemoteIpAddress;
            if (remoteIp != null)
                return remoteIp.MapToIPv4().ToString();
        }
        return null;
    }
}