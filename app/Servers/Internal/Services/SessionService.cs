namespace app.Services;


public class SessionService
{
    private readonly Dictionary<string, (int userId, decimal balance)> _sessions = new()
    {
        { "abcd1234", (123, 1000) }
    };

    public SessionVerifyResponse VerifySession(SessionVerifyRequest req)
    {
        if (!_sessions.ContainsKey(req.SessionToken))
            return new SessionVerifyResponse(false, 0, 0);

        var (userId, balance) = _sessions[req.SessionToken];
        return new SessionVerifyResponse(true, userId, balance);
    }
}
