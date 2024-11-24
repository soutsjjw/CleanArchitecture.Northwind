using System.Security.Claims;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);

    string GenerateRefreshToken();
}
