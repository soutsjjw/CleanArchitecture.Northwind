using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.User.Queries.UserDetail;
using CleanArchitecture.Northwind.Domain.Enums;

namespace CleanArchitecture.Northwind.Application.Features.User.Commands.UpdateUser;

public record UpdateUserCommand : IRequest<Result>
{
    public string UserId { get; set; }

    public string FullName { get; set; }

    public string? IDNo { get; set; }

    public string Title { get; set; }

    public int DepartmentId { get; set; }

    public int OfficeId { get; set; }

    public Status Status { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool EmailConfirmed { get; set; }
}
