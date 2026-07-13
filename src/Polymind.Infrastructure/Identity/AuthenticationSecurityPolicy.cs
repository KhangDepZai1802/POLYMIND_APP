namespace Polymind.Infrastructure.Identity;

/// <summary>Các quy tắc xác thực dùng chung cho web cookie và REST API.</summary>
public static class AuthenticationSecurityPolicy
{
    /// <summary>Phản hồi công khai chung, không tiết lộ email có tồn tại, bị khóa hay lockout.</summary>
    public const string InvalidCredentialsMessage = "Email hoặc mật khẩu không đúng.";

    public static bool IsSessionValid(
        bool isActive,
        bool supportsSecurityStamp,
        string? principalStamp,
        string? storedStamp)
        => isActive && (!supportsSecurityStamp
            || string.Equals(principalStamp, storedStamp, StringComparison.Ordinal));
}
