using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserAPILogin;

public class UserAPILoginVm
{
    public string UserName { get; set; }

    public string FullName { get; set; }

    public string Title { get; set; }

    public string TokenType { get; } = "Bearer";

    public string AccessToken { get; init; }

    public long ExpiresIn { get; init; }

    public string RefreshToken { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AccessTokenResponse, UserAPILoginVm>();
        }
    }
}
