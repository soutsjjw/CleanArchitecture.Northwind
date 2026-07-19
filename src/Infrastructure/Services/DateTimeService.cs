using CleanArchitecture.Northwind.Application.Common.Interfaces;

namespace CleanArchitecture.Northwind.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
}
