using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserLogin;

public class UserLoginVm
{
    public string UserName { get; set; }

    public string FullName { get; set; }

    public string IDNo { get; set; }

    public string Gender { get; set; }

    public string Title { get; set; }

    public string Status { get; set; }

    public ApplicationUser User { get; set; }

    public SignInResult SignInResult { get; set; }
}
