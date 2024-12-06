using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Account.Commands.Refresh;

public class RefreshVm
{
    //TODO: 這裡要找方法把 "Bearer" 替換為 JwtBearerDefaults.AuthenticationScheme

    public string TokenType { get; } = "Bearer";

    public required string AccessToken { get; init; }

    public required long ExpiresIn { get; init; }

    public required string RefreshToken { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AccessTokenResponse, RefreshVm>();
        }
    }
}
