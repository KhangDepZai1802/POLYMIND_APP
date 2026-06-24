using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Polymind.Domain.Entities;
using Polymind.Infrastructure.Persistence;

namespace Polymind.Web.Auditing;

public static class AuditLogHelpers
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<Guid?> GetUserIdAsync(this AuthenticationStateProvider authStateProvider)
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsed) ? parsed : null;
    }

    public static async Task<Guid> GetRequiredUserIdAsync(
        this AuthenticationStateProvider authStateProvider,
        ApplicationDbContext db)
    {
        var userId = await authStateProvider.GetUserIdAsync();
        if (userId is not null)
            return userId.Value;

        return await db.Users.Select(u => u.Id).FirstAsync();
    }

    public static void AddAudit(
        this ApplicationDbContext db,
        Guid? userId,
        string action,
        string resource,
        Guid? resourceId,
        object? oldValue = null,
        object? newValue = null)
    {
        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            Resource = resource,
            ResourceId = resourceId,
            OldValue = Serialize(oldValue),
            NewValue = Serialize(newValue),
        });
    }

    private static string? Serialize(object? value)
        => value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
}
