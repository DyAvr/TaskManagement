using HomeworkApp.Bll.Services.Interfaces;
using HomeworkApp.Dal.Repositories.Interfaces;

namespace HomeworkApp.Bll.Services;

public class RateLimitService : IRateLimitService
{
    private readonly IRateLimitRepository _rateLimitRepository;
    private const int rateLimit = 100; 

    public RateLimitService(IRateLimitRepository rateLimitRepository)
    {
        _rateLimitRepository = rateLimitRepository;
    }
    public async Task<bool> IsLimitExceeded(string ipAddress, CancellationToken token)
    {
        return await _rateLimitRepository.IsLimitExceeded(ipAddress, rateLimit, token);
    }
}