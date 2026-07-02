using Polymind.Domain.Enums;

namespace Polymind.Web.Display;

/// <summary>
/// Quy tắc nhắc chăm sóc lead (góp ý Vietgroup 01/07/2026): lead đứng yên ở một trạng thái
/// quá ngưỡng giờ cho phép thì nhắc người phụ trách. Rule-based, không cần AI.
/// Dùng chung cho job nhắc việc (NotificationService), danh sách /leads và trang chi tiết lead.
/// </summary>
public static class LeadCareRules
{
    /// <summary>
    /// Ngưỡng giờ tối đa lead được phép đứng yên ở mỗi trạng thái trước khi nhắc.
    /// Null = trạng thái kết thúc, không nhắc.
    /// </summary>
    public static int? ThresholdHours(LeadStatus status) => status switch
    {
        LeadStatus.New => 24,          // lead mới phải được tiếp nhận trong 24h
        LeadStatus.NotContacted => 24, // chưa liên hệ quá 24h = người phụ trách đang quên
        LeadStatus.Contacted => 48,    // đã liên hệ nhưng lead chưa phản hồi quan tâm → gọi lại
        LeadStatus.Interested => 48,   // quan tâm rồi thì phải chốt lịch hẹn tư vấn
        LeadStatus.Appointment => 24,  // tính từ GIỜ HẸN (không phải lần đổi trạng thái) — xem TryGetOverdue
        LeadStatus.Consulting => 72,
        LeadStatus.Registered => 72,
        _ => null // Converted / Unsuitable / Cancelled
    };

    /// <summary>Việc kế tiếp cần làm ở mỗi trạng thái — hiện trong nội dung nhắc.</summary>
    public static string NextAction(LeadStatus status) => status switch
    {
        LeadStatus.New => "cần tiếp nhận và liên hệ lead lần đầu",
        LeadStatus.NotContacted => "cần gọi/nhắn cho lead rồi chuyển sang \"Đã liên hệ\"",
        LeadStatus.Contacted => "lead chưa phản hồi quan tâm — cần gọi lại chăm sóc",
        LeadStatus.Interested => "lead đang quan tâm — cần đặt lịch hẹn tư vấn",
        LeadStatus.Appointment => "đã qua giờ hẹn — cần cập nhật kết quả tư vấn",
        LeadStatus.Consulting => "tư vấn kéo dài — cần chốt đăng ký hoặc cập nhật trạng thái",
        LeadStatus.Registered => "đã đăng ký — cần chuyển thành ứng viên để vào quy trình",
        _ => "cần cập nhật trạng thái"
    };

    /// <summary>
    /// Lead có đang quá hạn chăm sóc không. Trạng thái "Hẹn tư vấn" tính từ giờ hẹn (nếu có);
    /// các trạng thái khác tính từ lần đổi trạng thái gần nhất (fallback ngày tạo lead).
    /// </summary>
    public static bool TryGetOverdue(
        LeadStatus status,
        DateTimeOffset lastStatusChangeAt,
        DateTimeOffset? appointmentAt,
        DateTimeOffset now,
        out int stalledHours)
    {
        stalledHours = 0;
        if (ThresholdHours(status) is not int threshold) return false;

        var anchor = status == LeadStatus.Appointment && appointmentAt is not null
            ? appointmentAt.Value
            : lastStatusChangeAt;
        if (anchor > now) return false; // lịch hẹn còn ở tương lai

        var stalled = now - anchor;
        if (stalled < TimeSpan.FromHours(threshold)) return false;
        stalledHours = (int)stalled.TotalHours;
        return true;
    }

    /// <summary>"5 giờ" / "3 ngày" — hiển thị khoảng thời gian đứng yên cho gọn.</summary>
    public static string DurationLabel(int hours) => hours >= 48 ? $"{hours / 24} ngày" : $"{hours} giờ";
}
