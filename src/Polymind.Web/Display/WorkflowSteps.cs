using Polymind.Domain.Enums;

namespace Polymind.Web.Display;

/// <summary>
/// Tiện ích cho quy trình 20 bước: số thứ tự hiển thị (bước phụ 7.5), bước kế tiếp,
/// và % tiến độ. Mọi chỗ hiển thị/điều hướng bước phải dùng class này thay vì ép (int).
/// </summary>
public static class WorkflowSteps
{
    /// <summary>Tổng số bước chính của quy trình (không tính bước phụ 7.5).</summary>
    public const int MainCount = 20;

    /// <summary>20 bước chính theo thứ tự — KHÔNG gồm bước phụ 7.5 (ReselectJobOrder).</summary>
    public static readonly WorkflowStep[] Main = Enum.GetValues<WorkflowStep>()
        .Where(s => s != WorkflowStep.ReselectJobOrder)
        .OrderBy(s => (int)s)
        .ToArray();

    /// <summary>Số thứ tự hiển thị: "1".."20", riêng bước phụ trả về "7.5".</summary>
    public static string No(WorkflowStep s) => s == WorkflowStep.ReselectJobOrder
        ? "7.5"
        : ((int)s > (int)WorkflowStep.ReselectJobOrder ? (int)s - 1 : (int)s).ToString();

    /// <summary>
    /// Bước kế tiếp trong luồng bình thường. Từ Khám sức khỏe (B7) nhảy thẳng sang
    /// Thi tuyển (B8) — bước 7.5 chỉ vào được qua luồng "rớt B8", và từ 7.5 cũng sang B8.
    /// </summary>
    public static WorkflowStep Next(WorkflowStep s) => s switch
    {
        WorkflowStep.HealthCheck => WorkflowStep.EntranceExam,
        >= WorkflowStep.Completed => WorkflowStep.Completed,
        _ => (WorkflowStep)((int)s + 1)
    };

    /// <summary>% tiến độ theo 20 bước chính; bước phụ 7.5 tính như đang ở bước 7.</summary>
    public static double Progress(WorkflowStep? s)
    {
        if (s is null) return 0;
        var idx = s == WorkflowStep.ReselectJobOrder
            ? (int)WorkflowStep.HealthCheck
            : ((int)s.Value > (int)WorkflowStep.ReselectJobOrder ? (int)s.Value - 1 : (int)s.Value);
        return idx * 100.0 / MainCount;
    }
}
