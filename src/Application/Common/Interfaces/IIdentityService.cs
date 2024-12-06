using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string> UserRegisterAsync(string userName, string password);

    Task<AccessTokenResponse> UserLoginByAPI(string userName, string password);

    Task<string> GenerateEmailConfirmationTokenAsync(string userId);

    Task<AccessTokenResponse> RefreshByAPI(string refreshToken);

    Task<bool> ConfirmEmailAsync(string email, string token);

    Task<bool> ResetPasswordAsync(string email, string resetCode, string newPassword);

    Task<string?> GetUserNameAsync(string userId);

    Task<string?> GetUserIdAsync(string userName);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<bool> SendConfirmationEmailAsync(string userId, string email, string confirmationLink);

    Task<bool> SendForgotPasswordEmailAsync(string userId, string email, string resetCodeLink);
}
