using Polymind.Domain.Enums;

namespace Polymind.Domain.Finance;

/// <summary>Luật thu tiền theo lịch 4 bước, tách khỏi UI để mọi đường ghi nhận Paid dùng chung.</summary>
public static class PaymentPostingRules
{
    /// <summary>Tổng số bước của lịch đóng tiền (Đặt cọc → Phí dịch vụ → Phí trước bay → Tất toán).</summary>
    public const int TotalStages = 4;

    /// <summary>
    /// Các bước TRƯỚC <paramref name="current"/> mà ứng viên chưa đóng — tức lý do cụ thể khiến
    /// không duyệt được bước hiện tại. Sắp xếp tăng dần để báo lỗi đọc theo đúng thứ tự.
    /// </summary>
    public static IReadOnlyList<PaymentStage> UnpaidEarlierStages(
        PaymentStage current,
        IEnumerable<(PaymentStage Stage, PaymentStatus Status)> siblings)
        => siblings
            .Where(x => (int)x.Stage < (int)current && x.Status != PaymentStatus.Paid)
            .Select(x => x.Stage)
            .Distinct()
            .OrderBy(x => (int)x)
            .ToList();

    public static bool HasUnpaidEarlierStage(
        PaymentStage current,
        IEnumerable<(PaymentStage Stage, PaymentStatus Status)> siblings)
        => UnpaidEarlierStages(current, siblings).Count > 0;

    /// <summary>Ứng viên đã nộp bước này chưa — đã nộp chờ duyệt HOẶC kế toán đã duyệt.</summary>
    public static bool IsSubmittedOrPaid(PaymentStatus status)
        => status is PaymentStatus.Submitted or PaymentStatus.Paid;

    /// <summary>
    /// Các bước TRƯỚC <paramref name="current"/> mà ứng viên chưa NỘP — dùng cho thao tác "xác nhận đã nộp"
    /// bên Tiến độ đóng tiền. Nhẹ hơn <see cref="UnpaidEarlierStages"/>: không bắt kế toán phải duyệt xong
    /// bước trước mới cho ứng viên nộp bước sau, chỉ bắt nộp đúng thứ tự 1→4.
    /// </summary>
    public static IReadOnlyList<PaymentStage> UnsubmittedEarlierStages(
        PaymentStage current,
        IEnumerable<(PaymentStage Stage, PaymentStatus Status)> siblings)
        => siblings
            .Where(x => (int)x.Stage < (int)current && !IsSubmittedOrPaid(x.Status))
            .Select(x => x.Stage)
            .Distinct()
            .OrderBy(x => (int)x)
            .ToList();

    /// <summary>
    /// Chỉ được đưa lịch đóng tiền của ứng viên vào kho lưu trữ khi đã thu ĐỦ CẢ 4 BƯỚC (Paid).
    /// Chốt chặn để lưu trữ không bị dùng như một đường miễn nợ trá hình: còn thiếu 1 đồng là không lưu trữ được.
    /// </summary>
    public static bool CanArchiveSchedule(IEnumerable<(PaymentStage Stage, PaymentStatus Status)> stages)
    {
        var paid = stages
            .Where(x => x.Status == PaymentStatus.Paid)
            .Select(x => x.Stage)
            .Distinct()
            .Count();

        return paid == TotalStages;
    }
}
