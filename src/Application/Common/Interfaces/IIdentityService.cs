using System.Security.Claims;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<ApplicationUser?> GetUserByIdAsync(string userId);

    Task<string> UserRegisterAsync(string userName, string password);

    Task<(SignInResult? Result, ApplicationUser User)> UserLogin(string userName, string password, bool useCookies);

    Task<(SignInResult, AccessTokenResponse? token)> UserLoginByAPI(string userName, string password);

    Task SignInAsync(ApplicationUser user, bool useCookies);

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

    Task<bool> SendConfirmationEmailAsync(string userId, string email);

    Task<bool> SendForgotPasswordEmailAsync(string userId, string email, string resetCodeLink);

    Task<AccessTokenResponse> GenerateTokenResponseAsync(ApplicationUser user);

    /// <summary>
    /// 非同步註銷目前使用者。
    /// </summary>
    /// <remarks>此方法清除使用者的驗證工作階段及所有關聯的 Cookie。此方法應在使用者選擇退出應用程式時呼叫。</remarks>
    /// <returns>表示非同步退出操作的任務。 </returns>
    Task SignOutAsync();

    /// <summary>
    /// 確定指定使用者目前是否已登入。
    /// </summary>
    /// <param name="user"><see cref="ClaimsPrincipal"/> 代表要檢查的使用者。</param>
    /// <returns>如果使用者已登入為 <see langword="true"/>；否則 <see langword="false"/>.</returns>
    bool IsSignedIn(ClaimsPrincipal user);
}
