namespace HomeworkApp.Dal.Repositories.Interfaces;

public interface IRateLimitRepository
{
    Task<bool> IsLimitExceeded(string ipAddress, int limit, CancellationToken token);
}