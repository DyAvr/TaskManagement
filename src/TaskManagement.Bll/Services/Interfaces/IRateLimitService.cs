namespace HomeworkApp.Bll.Services.Interfaces;

public interface IRateLimitService
{
    Task<bool> IsLimitExceeded(string ipAddress, CancellationToken token);
}