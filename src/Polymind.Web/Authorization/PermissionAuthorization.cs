using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Polymind.Infrastructure.Persistence;
using Polymind.Infrastructure.Persistence.Constants;

namespace Polymind.Web.Authorization;

public static class PermissionClaimTypes
{
    public const string Permission = "permission";
}

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasPermission = context.User.Claims.Any(claim =>
            claim.Type == PermissionClaimTypes.Permission
            && string.Equals(claim.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission || context.User.IsInRole(RoleNames.SuperAdmin))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

public sealed class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var configuredPolicy = await base.GetPolicyAsync(policyName);
        if (configuredPolicy is not null)
            return configuredPolicy;

        if (!IsPermissionPolicyName(policyName))
            return null;

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }

    private static bool IsPermissionPolicyName(string policyName)
    {
        var parts = policyName.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 2
            && PermissionRegistry.Resources.Contains(parts[0], StringComparer.OrdinalIgnoreCase)
            && PermissionRegistry.Actions.Contains(parts[1], StringComparer.OrdinalIgnoreCase);
    }
}
