using Polymind.Infrastructure.Identity;
using Xunit;

namespace Polymind.Tests;

/// <summary>Regression cho BUG_M01_01/TC_M01_020 và BUG_M01_02/TC_M01_006.</summary>
public class M01_AuthenticationSecurityPolicyTests
{
    [Fact]
    public void Inactive_account_session_is_invalid_even_when_security_stamp_matches()
    {
        var result = AuthenticationSecurityPolicy.IsSessionValid(
            isActive: false,
            supportsSecurityStamp: true,
            principalStamp: "same-stamp",
            storedStamp: "same-stamp");

        Assert.False(result);
    }

    [Fact]
    public void Active_account_without_security_stamp_support_remains_valid()
    {
        var result = AuthenticationSecurityPolicy.IsSessionValid(
            isActive: true,
            supportsSecurityStamp: false,
            principalStamp: null,
            storedStamp: null);

        Assert.True(result);
    }

    [Theory]
    [InlineData("same-stamp", "same-stamp", true)]
    [InlineData("old-stamp", "new-stamp", false)]
    [InlineData(null, "stored-stamp", false)]
    public void Active_account_requires_matching_security_stamp(
        string? principalStamp,
        string? storedStamp,
        bool expected)
    {
        var result = AuthenticationSecurityPolicy.IsSessionValid(
            isActive: true,
            supportsSecurityStamp: true,
            principalStamp,
            storedStamp);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Public_failure_message_does_not_reveal_lock_state()
    {
        var message = AuthenticationSecurityPolicy.InvalidCredentialsMessage;

        Assert.DoesNotContain("khóa", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tồn tại", message, StringComparison.OrdinalIgnoreCase);
    }
}
