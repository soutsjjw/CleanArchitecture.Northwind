using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitecture.Northwind.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptionSettings _jwtOptionSettings;

    public JwtTokenService(IOptions<JwtOptionSettings> jwtOptionSettings)
    {
        _jwtOptionSettings = jwtOptionSettings.Value;
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptionSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var issuer = _jwtOptionSettings.Issuer;
        var audience = _jwtOptionSettings.Audience;

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtOptionSettings.ExpiresInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken(string userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptionSettings.RefreshKey));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            //ValidateIssuer = false,
            //ValidateAudience = false,
            //ValidateLifetime = false,
            //ValidateIssuerSigningKey = true,
            //IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptionSettings.RefreshKey)),
            //ClockSkew = TimeSpan.Zero

            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptionSettings.Issuer,
            ValidAudience = _jwtOptionSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptionSettings.Key)),
            // 指定從 JWT 聲明中提取哪個字段作為用戶名
            NameClaimType = "name",
            ValidAlgorithms = new string[] { "HS512" },
            // 允許服務器和客戶端之間的時間不同步，避免因小的時間偏移而導致 JWT 驗證失敗
            ClockSkew = TimeSpan.Zero,
        }, out SecurityToken securityToken);

        var jwtToken = securityToken as JwtSecurityToken;
        if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");
        return principal;
    }
}
