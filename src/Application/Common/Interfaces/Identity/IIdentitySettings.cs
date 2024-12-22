namespace CleanArchitecture.Northwind.Application.Common.Interfaces.Identity;

public interface IIdentitySettings
{
    // Password settings.
    /// <summary>
    /// 密碼是否需要數字
    /// </summary>
    bool RequireDigit { get; }

    /// <summary>
    /// 密碼所需的最小長度
    /// </summary>
    int RequiredLength { get; }

    /// <summary>
    /// 密碼所需的最大長度
    /// </summary>
    int MaxLength { get; }

    /// <summary>
    /// 密碼是否需要非字母數字字符
    /// </summary>
    bool RequireNonAlphanumeric { get; }

    /// <summary>
    /// 密碼是否需要大寫字母
    /// </summary>
    bool RequireUpperCase { get; }

    /// <summary>
    /// 密碼是否需要小寫字母
    /// </summary>
    bool RequireLowerCase { get; }

    /// <summary>
    /// 密碼需要的唯一字符數量
    /// </summary>
    int RequiredUniqueChars { get; }

    // Lockout settings.
    /// <summary>
    /// 鎖定時間
    /// </summary>
    int DefaultLockoutTimeSpan { get; }

    /// <summary>
    /// 登入失敗次數
    /// </summary>
    int MaxFailedAccessAttempts { get; }

    /// <summary>
    /// 新使用者是否可以被鎖定
    /// </summary>
    bool AllowedForNewUsers { get; }

    /// <summary>
    /// 使用者註冊確認網址
    /// </summary>
    string ConfirmEmailTokenUrl { get; }
}
