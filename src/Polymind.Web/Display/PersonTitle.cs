using Polymind.Domain.Enums;

namespace Polymind.Web.Display;

/// <summary>
/// Danh xưng người trong hồ sơ theo tiến trình + phân nhóm đơn hàng (góp ý Vietgroup 07/2026):
/// từ khi ĐẶT CỌC (B5) đổi khỏi "Ứng viên"; khi đó đơn hàng nhóm <b>Du học</b> → "Học viên",
/// các nhóm còn lại (ngoài nước / trong nước) → "Lao động".
/// </summary>
public static class PersonTitle
{
    /// <summary>Đã qua mốc Đặt cọc → được gắn danh xưng Học viên/Lao động (không còn là "Ứng viên").</summary>
    public static bool IsTitled(WorkflowStep? step)
        => step is not null && (int)step.Value >= (int)WorkflowStep.Deposit;

    /// <summary>Học viên = đơn hàng phân nhóm Du học và đã qua mốc Đặt cọc.</summary>
    public static bool IsStudent(WorkflowStep? step, JobCategory? category)
        => IsTitled(step) && category == JobCategory.StudyAbroad;

    public static string Of(WorkflowStep? step, JobCategory? category)
    {
        if (!IsTitled(step)) return "Ứng viên";
        return category == JobCategory.StudyAbroad ? "Học viên" : "Lao động";
    }
}
