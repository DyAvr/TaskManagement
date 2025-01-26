using HomeworkApp.Dal.Providers.Interfaces;

namespace HomeworkApp.Dal.Providers;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset Now() => DateTimeOffset.UtcNow;
}