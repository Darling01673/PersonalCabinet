using Microsoft.Extensions.Caching.Memory;

namespace PersonalCabinet.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path.ToString();
            bool isExcluded = false;
            if (path.StartsWith("/css/")) isExcluded = true;
            if (path.StartsWith("/js/")) isExcluded = true;
            if (path.StartsWith("/images/")) isExcluded = true;
            if (path.StartsWith("/lib/")) isExcluded = true;
            if (path.StartsWith("/Home/RateLimit")) isExcluded = true;
            if  (path.StartsWith("/chatHub")) isExcluded = true;
            if (path.StartsWith("/Admin/GetNewMessages")) isExcluded = true;

            if (isExcluded)
            {
                await _next(context);
                return;
            }

            string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            string countKey = $"rate_count_{clientIp}";
            string unlockKey = $"rate_unlock_{clientIp}";
            if (_cache.TryGetValue(unlockKey, out DateTime unlockTime) && unlockTime > DateTime.UtcNow)
            {
                int remainingSeconds = (int)(unlockTime - DateTime.UtcNow).TotalSeconds;
                if (remainingSeconds < 1) remainingSeconds = 1;
                context.Response.Redirect($"/Home/RateLimit?seconds={remainingSeconds}");
                return;
            }
            if (!_cache.TryGetValue(countKey, out int requestCount))
            {
                _cache.Set(countKey, 1, TimeSpan.FromSeconds(60));
                await _next(context);
                return;
            }

            if (requestCount >= 100) 
            {
                _cache.Set(unlockKey, DateTime.UtcNow.AddSeconds(60), TimeSpan.FromSeconds(60));
                _cache.Remove(countKey);
                context.Response.Redirect("/Home/RateLimit?seconds=60");
                return;
            }
            _cache.Set(countKey, requestCount + 1, TimeSpan.FromSeconds(60));
            await _next(context);
        }
    }
}