namespace CleanArchitecture.Northwind.Application.Common.Logging;

public static class LoggingEvents
{
    public static class Account
    {
        public static class Login
        {
            public const string UserNotFound = "使用不存在的帳號進行登入";
            public const string AccountLocked = "帳號已被鎖定";
            public const string EmailNotConfirmed = "Email 尚未認證";
            public const string InvalidLoginAttempt = "帳號或密碼錯誤";

            public const string UserNotFoundFormat = "使用不存在的帳號 {UserName} 進行登入";
            public const string AccountLockedFormat = "{UserName} 帳號已被鎖定";
            public const string EmailNotConfirmedFormat = "{UserName} Email 尚未認證";
            public const string InvalidLoginAttemptFormat = "{UserName} 帳號或密碼錯誤";
        }
    }
}
