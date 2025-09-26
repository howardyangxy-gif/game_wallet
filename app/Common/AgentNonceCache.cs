using System;
using System.Collections.Concurrent;
using System.Linq;

public static class RequestNonceCache
{
    private static readonly ConcurrentDictionary<string, DateTime> _cache = new();

    public static bool TryAdd(string nonce, DateTime now)
    {
        // 若已存在則回傳 false
        return _cache.TryAdd(nonce, now);
    }

    public static void RemoveOld(DateTime threshold)
    {
        var old = _cache.Where(kv => kv.Value < threshold).Select(kv => kv.Key).ToList();
        foreach (var key in old)
            _cache.TryRemove(key, out _);
    }
}