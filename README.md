Base:
.NET 8 SDK
MySQL 8.0
Redis 7.2

安裝指令:
docker-compose up -d
dotnet restore
dotnet build
dotnet run

開發順序:
*. 串接mysql ok
*. 簡易API ok, 
*. 營運商玩家TABLE ok
*. 轉帳錢包玩家上下分 ok (待補:錯誤馬回傳統一, wallet sp調整)
*. 轉帳錢包取得玩家餘額 ok
*. 服務端溝通下注+結算 接口
*. 代理端溝通下注+結算 接口
*. 服務端餘額 接口
*. 代理端餘額 接口
*. getgameurl (或註冊玩家)

下一階段
*. 幣別
*. redis串接
*. 交易單號存 redis 
*. 代理資料存 redis
*. rollbackBet


．營運商對接(驗證方式確認) 
    Security
    Each request between servers should be signed.
    Each request's signature should be validated.
    Signature of the request should be calculated using
    HMAC-SHA256
    algorithm, where message is request body and key is
    Signature should be sent in "X-REQUEST-SIGN" header of the request.
    In case of signature mismatch, server (GCP and wallet) should respond with HTTP 403 Forbidden

．營運商白名單限制
．API unittest

．Dates and Timestamps formats
    Dates and Timestamps should be formatted according to 

．Games List





．db MIGRATION
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet tool install --global dotnet-ef


玩家在營運商或遊戲畫面
=>前端登入 
=>打getGameUrl 或 getUserToken

*檢查玩家, 單一錢包=>打給代理做認證+拿餘額, 轉帳錢包=>認證  
*return gameUrl+token做轉導進入遊戲


單一錢包:
第一種(pg):
直接組遊戲端連結登入:
url+ gameCode + operatorToken + userToken + language
之後都是用這個userToken溝通
登入後:遊戲端發起Verify給代理(發TOKEN過去) 要收到Succes + USERNAME, CURRENCY
接下來都是用此userToken溝通, 遊戲端發起要餘額, 下注,結算(也有某些狀態是發不用userToken驗證個請求出去的, 是用USERNAME, 例如bet應該要驗證, result就不用)
 

第二種(pp): (v先做這種)
直接組遊戲連結登入:
url+ gameCode + operatorToken + userToken + language
登入後:遊戲端發起auth給代理(userToken), 要收到Succes + USERNAME, CURRENCY
接下來都是用USERNAME跟加解密溝通, 遊戲端發起要餘額, 下注,結算

第三種(fc):
第四種(cq9):


轉帳錢包:
玩家從主頁面或遊戲前端進入遊戲後, 


game => 與服務端溝通接口

<玩家登入>
B端=>checkUserToken=>錢包端=>checkUserToken=>代理
                     錢包端  <=   result   <=代理
                     錢包端  => getBalance =>代理
B端 <=  result   <=  錢包端  <=   result   <=代理

<玩家下注><單一錢包>
B端=>      Bet     =>錢包端=>      Bet     =>代理



<玩家下注><轉帳錢包>
B端=>      Bet     =>錢包端=>      Bet     =>代理





































