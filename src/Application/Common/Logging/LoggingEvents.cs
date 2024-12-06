namespace CleanArchitecture.Northwind.Application.Common.Logging;

public static class LoggingEvents
{
    public static class Account
    {
        public const string UserNotFound = "使用不存在的帳號進行登入";
        public const string UserNotFoundFormat = "使用不存在的帳號 {UserName} 進行登入";

        public const string AccountLocked = "帳號已被鎖定";
        public const string AccountLockedFormat = "{UserName} 帳號已被鎖定";

        public const string EmailNotConfirmed = "Email 尚未認證";
        public const string EmailNotConfirmedFormat = "{UserName} Email 尚未認證";

        public const string InvalidLoginAttempt = "帳號或密碼錯誤";
        public const string InvalidLoginAttemptFormat = "{UserName} 帳號或密碼錯誤";

        public const string AccountRegistrationFailed = "帳號註冊失敗";
        public const string AccountRegistrationFailedFormat = "{Email} 帳號註冊失敗";

        public const string SendConfirmLetterFailed = "寄送認證信件失敗";
        public const string SendConfirmLetterFailedFormat = "{Email} 寄送認證信件失敗";

        public const string SendConfirmLetterUseNonExistentEmailFormat = "使用不存在的 {Email} 寄送認證信件";

        public const string SendForgotPasswordLetterUseNonExistentEmailFormat = "使用不存在的 {Email} 寄送忘記密碼信件";
        public const string SendForgotPasswordLetterUseNonExistentUserFormat = "使用不存在的 {UserId} 寄送忘記密碼信件";

        public const string SendForgotPasswordLetterFailed = "寄送忘記密碼信件失敗";
        public const string SendForgotPasswordLetterFailedFormat = "{Email} 寄送忘記密碼信件失敗";

        public const string ResetPasswordUseNonExistentEmailFormat = "使用不存在的 {Email} 重設密碼";

        public const string InvalidClientToken = "無效的客戶端令牌";
    }
}
