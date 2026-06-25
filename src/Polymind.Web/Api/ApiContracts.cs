using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Polymind.Domain.Enums;

namespace Polymind.Web.Api;

// ---- Auth ----
public sealed record LoginRequest(string Email, string Password);

public sealed record UserInfo(
    Guid Id,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed record TokenResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    UserInfo User);

// ---- Chung ----
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);

// ---- Lead ----
public sealed record LeadDto(
    Guid Id, string Code, string FullName, string? Phone, string? Email,
    string? Province, string Source, string Status, DateTimeOffset CreatedAt);

public sealed record LeadCreateRequest(
    string FullName, string? Phone, string? Email, string? Province,
    string? Address, string? Occupation, string? TargetCountry,
    LeadSource Source, string? Notes);

public sealed record LeadUpdateRequest(
    string FullName, string? Phone, string? Email, string? Province,
    string? Address, string? Occupation, string? TargetCountry,
    LeadSource Source, LeadStatus Status, string? Notes);

// ---- Candidate ----
public sealed record CandidateDto(
    Guid Id, string Code, string FullName, string? Phone, string? Province,
    string? Gender, string? PassportNumber, DateTimeOffset CreatedAt);

// ---- Job order ----
public sealed record JobOrderDto(
    Guid Id, string Code, string Country, string? CompanyName, string? Field,
    int Quantity, decimal? CostAmount, string Status, DateOnly? ExpectedDepartureDate);

/// <summary>
/// Helper áp <c>[Authorize]</c> cho endpoint API: bắt buộc scheme JWT Bearer (để trả 401 thay vì
/// redirect cookie) + policy <c>resource:action</c> động. Không truyền policy = chỉ cần đăng nhập.
/// </summary>
public static class ApiAuth
{
    public static IAuthorizeData[] Bearer(string? policy = null) => new IAuthorizeData[]
    {
        new AuthorizeAttribute
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Policy = policy,
        }
    };

    public static Guid? UserId(this ClaimsPrincipal principal)
        => Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
