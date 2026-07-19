using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Member.Queries.GetProfile;

public record GetProfileQuery : IRequest<Result<ProfileVm>>
{
    public string UserId { get; set; }
}
