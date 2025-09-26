
public record SessionVerifyRequest(string SessionToken, int UserId);
public record SessionVerifyResponse(bool Valid, int UserId, decimal Balance);
