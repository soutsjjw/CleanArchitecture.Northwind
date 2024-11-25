using System.Security.Claims;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitecture.Northwind.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AccountController(
        IConfiguration configuration,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenService jwtTokenService)
    {
        _configuration = configuration;
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest login, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies)
    {
        var user = await _userManager.FindByEmailAsync(login.Email);

        if (user == null)
        {
            return Unauthorized("Invalid login attempt.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
        {
            if (!string.IsNullOrEmpty(login.TwoFactorCode))
            {
                result = await _signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, false, rememberClient: false);
            }
            else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
            {
                result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
            }
        }

        if (!result.Succeeded)
        {
            return Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
        }

        var tokenResponse = await GenerateTokenResponseAsync(user);

        return Ok(tokenResponse);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest refreshRequest)
    {
        if (refreshRequest is null)
        {
            return Unauthorized("Invalid Client Token.");
        }

        ClaimsPrincipal userPrincipal = null;
        string userId = string.Empty;

        try
        {
            userPrincipal = _jwtTokenService.GetPrincipalFromExpiredToken(refreshRequest.RefreshToken);
            userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        }
        catch (SecurityTokenSignatureKeyNotFoundException ex)
        {
            return Unauthorized("Invalid Client Token.");
        }
        catch (SecurityTokenMalformedException ex)
        {
            return Unauthorized("Invalid Client Token.");
        }

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid Client Token.");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return Unauthorized("User Not Found.");

        if (user.RefreshToken != refreshRequest.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            return Unauthorized("Invalid Client Token.");

        var tokenResponse = await GenerateTokenResponseAsync(user);

        return Ok(tokenResponse);
    }

    private async Task<AccessTokenResponse> GenerateTokenResponseAsync(ApplicationUser user)
    {
        var token = _jwtTokenService.GenerateAccessToken(await GetClaimsAsync(user));
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        if (!int.TryParse(_configuration["JwtOptions:ExpiresInMinutes"], out int expiresInMinutes))
        {
            expiresInMinutes = 60;
        }

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(expiresInMinutes);

        await _userManager.UpdateAsync(user);

        var tokenResponse = new AccessTokenResponse
        {
            AccessToken = token,
            ExpiresIn = expiresInMinutes * 60,
            RefreshToken = refreshToken
        };

        return tokenResponse;
    }

    private async Task<IEnumerable<Claim>> GetClaimsAsync(ApplicationUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();
        var permissionClaims = new List<Claim>();
        foreach (var role in roles)
        {
            roleClaims.Add(new Claim(ClaimTypes.Role, role));
            var thisRole = await _roleManager.FindByNameAsync(role);
            var allPermissionsForThisRoles = await _roleManager.GetClaimsAsync(thisRole);
            permissionClaims.AddRange(allPermissionsForThisRoles);
        }

        var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
            }
        .Union(userClaims)
        .Union(roleClaims)
        .Union(permissionClaims);

        return claims;
    }
}
