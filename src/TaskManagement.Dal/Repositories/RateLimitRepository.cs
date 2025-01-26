using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using Microsoft.Extensions.Options;

namespace HomeworkApp.Dal.Repositories;

public class RateLimitRepository : RedisRepository, IRateLimitRepository
{
    public RateLimitRepository(IOptions<DalOptions> dalSettings) : base(dalSettings.Value)
    {
    }

    protected override string KeyPrefix => "ratelimit";
    protected override TimeSpan KeyTtl => TimeSpan.FromMinutes(1);

    public async Task<bool> IsLimitExceeded(string ipAddress, int limit, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var connection = await GetConnection();
        var key = GetKey(ipAddress);
        var currentCount = await connection.StringIncrementAsync(key);

        if (currentCount == 1)
        {
            await connection.KeyExpireAsync(key, KeyTtl);
        }

        return currentCount > limit;
    }
}