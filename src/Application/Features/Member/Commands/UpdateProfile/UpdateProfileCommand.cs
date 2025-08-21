using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Member.Queries.GetProfile;
using CleanArchitecture.Northwind.Domain.Enums;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UpdateProfile;

public record UpdateProfileCommand : IRequest<Result>
{
    public string UserId { get; set; }

    public string FullName { get; set; }

    public string? IDNo { get; set; }

    public Gender Gender { get; set; }

    public string Title { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ProfileVm, UpdateProfileCommand>();
        }
    }
}
