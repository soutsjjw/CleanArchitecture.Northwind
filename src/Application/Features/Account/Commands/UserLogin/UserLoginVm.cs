using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserLogin;

public class UserLoginVm
{
    public string TokenType { get; } = "Bearer";

    public required string AccessToken { get; init; }

    public required long ExpiresIn { get; init; }

    public required string RefreshToken { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AccessTokenResponse, UserLoginVm>();
        }
    }
}
