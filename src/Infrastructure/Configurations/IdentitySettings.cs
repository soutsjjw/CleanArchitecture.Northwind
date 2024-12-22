using CleanArchitecture.Northwind.Application.Common.Interfaces.Identity;

namespace CleanArchitecture.Northwind.Infrastructure.Configurations;

public class IdentitySettings : IIdentitySettings
{
    /// <summary>
    ///     Identity settings key constraint
    /// </summary>
    public const string Key = nameof(IdentitySettings);

    // Password settings.
    /// <summary>
    /// 密碼是否需要數字
    /// </summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>
    /// 密碼所需的最小長度
    /// </summary>
    public int RequiredLength { get; set; } = 12;

    /// <summary>
    /// 密碼所需的最大長度
    /// </summary>
    public int MaxLength { get; set; } = 16;

    /// <summary>
    /// 密碼是否需要非字母數字字符
    /// </summary>
    public bool RequireNonAlphanumeric { get; set; } = true;

    /// <summary>
    /// 密碼是否需要大寫字母
    /// </summary>
    public bool RequireUpperCase { get; set; } = true;

    /// <summary>
    /// 密碼是否需要小寫字母
    /// </summary>
    public bool RequireLowerCase { get; set; } = false;

    /// <summary>
    /// 密碼需要的唯一字符數量
    /// </summary>
    public int RequiredUniqueChars { get; set; } = 1;

    // Lockout settings.
    /// <summary>
    /// 鎖定時間
    /// </summary>
    public int DefaultLockoutTimeSpan { get; set; } = 30;

    /// <summary>
    /// 登入失敗次數
    /// </summary>
    public int MaxFailedAccessAttempts { get; set; } = 5;

    /// <summary>
    /// 新使用者是否可以被鎖定
    /// </summary>
    public bool AllowedForNewUsers { get; set; } = true;

    /// <summary>
    /// 使用者註冊確認網址
    /// </summary>
    public string ConfirmEmailTokenUrl { get; set; } = "";
}
