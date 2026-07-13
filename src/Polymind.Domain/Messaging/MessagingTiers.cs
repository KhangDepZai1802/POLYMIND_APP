namespace Polymind.Domain.Messaging;

/// <summary>Bậc phân quyền nhắn tin. Số càng nhỏ = càng cao.</summary>
public enum MessagingTier
{
    SuperAdmin = 1,
    Director = 2,
    Operations = 3, // Kế toán, TP tuyển dụng, Bộ phận hồ sơ, Bộ phận visa
    Field = 4,      // Tư vấn viên, NV tuyển dụng, Đại lý
    Portal = 5,     // Phụ huynh, Học viên, CTV
}

/// <summary>
/// Ma trận "ai được nhắn ai" theo bậc (user chốt 2026-07-13).
///
/// <para><b>Tài liệu gốc: <c>docs/messaging-tiers.md</c></b> — đọc file đó trước khi sửa luật ở đây.</para>
///
/// Bốn mệnh đề:
/// <list type="number">
///   <item>Super Admin nhắn được tất cả, và ai cũng nhắn được Super Admin (kênh hỗ trợ).</item>
///   <item>Chênh bậc ≤ 1 thì được nhắn; chênh ≥ 2 thì chặn (2↔4, 2↔5, 3↔5 bị chặn).</item>
///   <item>Ba ngoại lệ chặn: TVV✗TVV · CTV✗CTV · Đại lý ✗ mọi role bậc 4 (đại lý là đối thủ của nhau
///         và là đối tác NGOÀI công ty).</item>
///   <item>Ma trận chỉ quyết định <i>loại role</i>. Việc <i>đúng người nào</i> do tầng quan hệ ứng viên
///         quyết định (xem <see cref="CandidateMessagingRelationship"/> + <c>Messages.razor</c>) —
///         ma trận KHÔNG thay thế được nó.</item>
/// </list>
///
/// Fail-closed: vai trò rỗng hoặc không nằm trong <see cref="TierByRole"/> → không nhắn được.
/// </summary>
public static class MessagingTiers
{
    // Domain không tham chiếu được Polymind.Infrastructure (nơi có RoleNames) nên phải dùng string literal.
    // `M14_MessagingMatrixTests.Tier_map_covers_every_role` khóa map này khớp RoleNames.All → không lệch âm thầm.
    public const string SuperAdmin = "super_admin";
    public const string Director = "director";
    public const string RecruitmentManager = "recruitment_manager";
    public const string Recruiter = "recruiter";
    public const string Consultant = "consultant";
    public const string DocumentStaff = "document_staff";
    public const string VisaStaff = "visa_staff";
    public const string Accountant = "accountant";
    public const string Agent = "agent";
    public const string Collaborator = "collaborator";
    public const string Parent = "parent";
    public const string Student = "student";

    public static readonly IReadOnlyDictionary<string, MessagingTier> TierByRole =
        new Dictionary<string, MessagingTier>(StringComparer.OrdinalIgnoreCase)
        {
            [SuperAdmin] = MessagingTier.SuperAdmin,

            [Director] = MessagingTier.Director,

            [Accountant] = MessagingTier.Operations,
            [RecruitmentManager] = MessagingTier.Operations,
            [DocumentStaff] = MessagingTier.Operations,
            [VisaStaff] = MessagingTier.Operations,

            [Consultant] = MessagingTier.Field,
            [Recruiter] = MessagingTier.Field,
            [Agent] = MessagingTier.Field,

            [Parent] = MessagingTier.Portal,
            [Student] = MessagingTier.Portal,
            [Collaborator] = MessagingTier.Portal,
        };

    /// <summary>Bậc hiệu lực của user = bậc CAO NHẤT trong các role của họ (số nhỏ nhất). Null nếu không có role hợp lệ.</summary>
    public static MessagingTier? EffectiveTier(IEnumerable<string> roles)
    {
        MessagingTier? best = null;
        foreach (var role in roles)
        {
            if (!TierByRole.TryGetValue(role, out var tier)) continue;
            if (best is null || tier < best) best = tier;
        }

        return best;
    }

    /// <summary>Role quyết định bậc hiệu lực (dùng để áp 3 ngoại lệ chặn). Null nếu không có role hợp lệ.</summary>
    private static string? EffectiveRole(IEnumerable<string> roles)
    {
        string? bestRole = null;
        MessagingTier? bestTier = null;
        foreach (var role in roles)
        {
            if (!TierByRole.TryGetValue(role, out var tier)) continue;
            if (bestTier is null || tier < bestTier)
            {
                bestTier = tier;
                bestRole = role;
            }
        }

        return bestRole;
    }

    /// <summary>
    /// Người có vai trò <paramref name="senderRoles"/> có được nhắn cho người có vai trò
    /// <paramref name="recipientRoles"/> không? Đối xứng theo thiết kế.
    /// </summary>
    public static bool CanMessage(IReadOnlyCollection<string> senderRoles, IReadOnlyCollection<string> recipientRoles)
    {
        var sender = EffectiveRole(senderRoles);
        var recipient = EffectiveRole(recipientRoles);
        if (sender is null || recipient is null) return false; // fail-closed

        var senderTier = TierByRole[sender];
        var recipientTier = TierByRole[recipient];

        // (1) Super Admin: hai chiều, không giới hạn.
        if (senderTier == MessagingTier.SuperAdmin || recipientTier == MessagingTier.SuperAdmin) return true;

        // (3) Ngoại lệ chặn — đè lên quy tắc chênh bậc.
        if (IsBlockedPair(sender, recipient, senderTier, recipientTier)) return false;

        // (2) Chênh bậc ≤ 1.
        return Math.Abs((int)senderTier - (int)recipientTier) <= 1;
    }

    private static bool IsBlockedPair(string a, string b, MessagingTier tierA, MessagingTier tierB)
    {
        // TVV không nhắn TVV khác; CTV không nhắn CTV khác.
        if (Is(a, Consultant) && Is(b, Consultant)) return true;
        if (Is(a, Collaborator) && Is(b, Collaborator)) return true;

        // Đại lý bị cô lập khỏi TOÀN BỘ bậc 4: đại lý ✗ đại lý (đối thủ), đại lý ✗ TVV, đại lý ✗ NV tuyển dụng.
        // Đại lý vẫn nhắn được bậc 3 (lên) và CTV của mình (xuống) — hai chiều đó chênh 1 bậc.
        if (Is(a, Agent) && tierB == MessagingTier.Field) return true;
        if (Is(b, Agent) && tierA == MessagingTier.Field) return true;

        return false;
    }

    private static bool Is(string role, string expected)
        => string.Equals(role, expected, StringComparison.OrdinalIgnoreCase);
}
