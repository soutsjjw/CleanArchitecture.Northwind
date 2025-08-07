using CleanArchitecture.Northwind.Domain.Entities.Identity;
using CleanArchitecture.Northwind.Domain.Enums;

namespace CleanArchitecture.Northwind.Application.Features.Member.Queries.GetProfile;

public class ProfileVm
{
    public string UserId { get; set; }

    public string FullName { get; set; }

    public string? IDNo { get; set; }

    public Gender Gender { get; set; }

    public string Title { get; set; }

    public bool IsTotpEnabled { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationUserProfile, ProfileVm>();
        }
    }
}
