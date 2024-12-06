using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string> UserRegisterAsync(string userName, string password);

    Task<AccessTokenResponse> UserLoginByAPI(string userName, string password);

    Task<string> GenerateEmailConfirmationTokenAsync(string userId);

    Task<AccessTokenResponse> RefreshByAPI(string refreshToken);

    Task<bool> ConfirmEmail(string email, string token);

    Task<bool> ResendConfirmationEmail(string email);

    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<bool> SendConfirmationEmailAsync(string userId, string email);
}
