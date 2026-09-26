using CleanArchitecture.Northwind.Application.Common.Interfaces;

namespace CleanArchitecture.Northwind.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(
        DateTime.UtcNow,
        GetTaipeiTimeZone());

    private static TimeZoneInfo GetTaipeiTimeZone()
    {
        if (TimeZoneInfo.TryFindSystemTimeZoneById("Asia/Taipei", out var taipeiTimeZone))
        {
            return taipeiTimeZone;
        }

        return TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
    }
}
