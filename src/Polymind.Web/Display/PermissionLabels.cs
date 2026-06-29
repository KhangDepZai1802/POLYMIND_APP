namespace Polymind.Web.Display;

/// <summary>
/// Nhãn tiếng Việt + mô tả dễ hiểu cho quyền RBAC (resource:action),
/// dùng cho màn Phân quyền để super admin không rành kỹ thuật vẫn đọc được.
/// </summary>
public static class PermissionLabels
{
    /// <summary>Tên module tiếng Việt theo resource. Thứ tự dùng để sắp xếp hiển thị.</summary>
    private static readonly IReadOnlyList<(string Resource, string Name)> ResourceOrder = new[]
    {
        ("leads", "Khách hàng tiềm năng (Lead)"),
        ("candidates", "Ứng viên"),
        ("job_orders", "Đơn hàng tuyển dụng"),
        ("payments", "Khoản thu"),
        ("expenses", "Khoản chi"),
        ("receipts", "Phiếu thu / chi"),
        ("agents", "Đại lý / Cộng tác viên"),
        ("commissions", "Hoa hồng đại lý"),
        ("loans", "Hỗ trợ vay vốn"),
        ("visas", "Hồ sơ Visa"),
        ("flights", "Vé máy bay / Xuất cảnh"),
        ("reports", "Báo cáo & Thống kê"),
        ("dashboard", "Bảng điều khiển (Dashboard)"),
        ("users", "Tài khoản người dùng"),
        ("roles", "Vai trò & Phân quyền"),
        ("notifications", "Thông báo"),
        ("audit", "Nhật ký hệ thống (Audit log)"),
    };

    private static readonly Dictionary<string, string> ResourceNames =
        ResourceOrder.ToDictionary(x => x.Resource, x => x.Name);

    /// <summary>Các hành động theo đúng thứ tự cột ma trận quyền.</summary>
    public static readonly IReadOnlyList<string> ActionOrder = new[]
    {
        "read", "create", "update", "delete", "approve"
    };

    private static readonly Dictionary<string, string> ActionNames = new()
    {
        ["read"] = "Xem",
        ["create"] = "Thêm mới",
        ["update"] = "Sửa",
        ["delete"] = "Xóa",
        ["approve"] = "Phê duyệt",
    };

    private static readonly Dictionary<string, string> ActionHints = new()
    {
        ["read"] = "Được mở và xem dữ liệu của mục này.",
        ["create"] = "Được thêm mới dữ liệu vào mục này.",
        ["update"] = "Được chỉnh sửa dữ liệu đã có.",
        ["delete"] = "Được xóa dữ liệu khỏi mục này.",
        ["approve"] = "Được phê duyệt (vd: duyệt khoản thu, duyệt hoa hồng).",
    };

    /// <summary>Tên module tiếng Việt; nếu chưa khai báo thì trả lại tên gốc.</summary>
    public static string Resource(string resource)
        => ResourceNames.GetValueOrDefault(resource, resource);

    /// <summary>Tên hành động tiếng Việt.</summary>
    public static string Action(string action)
        => ActionNames.GetValueOrDefault(action, action);

    /// <summary>Mô tả ngắn cho hành động (dùng cho chú thích/tooltip).</summary>
    public static string ActionHint(string action)
        => ActionHints.GetValueOrDefault(action, action);

    /// <summary>Thứ tự sắp xếp module cho ổn định (module lạ đẩy xuống cuối).</summary>
    public static int ResourceRank(string resource)
    {
        for (var i = 0; i < ResourceOrder.Count; i++)
            if (ResourceOrder[i].Resource == resource) return i;
        return int.MaxValue;
    }
}
