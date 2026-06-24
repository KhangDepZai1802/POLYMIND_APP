using MudBlazor;
using Polymind.Domain.Enums;

namespace Polymind.Web.Display;

/// <summary>Nhãn tiếng Việt + màu hiển thị cho các enum nghiệp vụ.</summary>
public static class Labels
{
    public static string Vi(LeadStatus s) => s switch
    {
        LeadStatus.New => "Lead mới",
        LeadStatus.NotContacted => "Chưa liên hệ",
        LeadStatus.Contacted => "Đã liên hệ",
        LeadStatus.Interested => "Quan tâm",
        LeadStatus.Appointment => "Hẹn tư vấn",
        LeadStatus.Consulting => "Đang tư vấn",
        LeadStatus.Registered => "Đăng ký",
        LeadStatus.Converted => "Đã chuyển ứng viên",
        LeadStatus.Unsuitable => "Không phù hợp",
        LeadStatus.Cancelled => "Hủy",
        _ => s.ToString()
    };

    public static Color ColorOf(LeadStatus s) => s switch
    {
        LeadStatus.New => Color.Info,
        LeadStatus.Contacted or LeadStatus.Interested or LeadStatus.Appointment or LeadStatus.Consulting => Color.Primary,
        LeadStatus.Registered => Color.Secondary,
        LeadStatus.Converted => Color.Success,
        LeadStatus.Unsuitable or LeadStatus.Cancelled => Color.Error,
        _ => Color.Default
    };

    public static string Vi(LeadSource s) => s switch
    {
        LeadSource.FacebookAds => "Facebook Ads",
        LeadSource.TiktokAds => "TikTok Ads",
        LeadSource.GoogleAds => "Google Ads",
        LeadSource.Website => "Website",
        LeadSource.LandingPage => "Landing Page",
        LeadSource.Zalo => "Zalo",
        LeadSource.Hotline => "Hotline",
        LeadSource.Agent => "Đại lý",
        LeadSource.Referral => "Giới thiệu",
        LeadSource.Event => "Sự kiện",
        _ => s.ToString()
    };

    public static string Vi(WorkflowStep s) => s switch
    {
        WorkflowStep.Lead => "Lead mới",
        WorkflowStep.Consulting => "Tư vấn",
        WorkflowStep.Registration => "Đăng ký",
        WorkflowStep.Deposit => "Đặt cọc",
        WorkflowStep.Document => "Hoàn thiện hồ sơ",
        WorkflowStep.HealthCheck => "Khám sức khỏe",
        WorkflowStep.Orientation => "Học định hướng",
        WorkflowStep.EntranceExam => "Thi tuyển",
        WorkflowStep.Selected => "Trúng tuyển",
        WorkflowStep.SignContract => "Ký hợp đồng",
        WorkflowStep.VisaSubmit => "Nộp hồ sơ Visa",
        WorkflowStep.VisaApproved => "Đậu Visa",
        WorkflowStep.FullPayment => "Thanh toán đủ",
        WorkflowStep.BookFlight => "Đặt vé máy bay",
        WorkflowStep.Departure => "Xuất cảnh",
        WorkflowStep.Arrived => "Đến nơi làm việc",
        WorkflowStep.Completed => "Hoàn tất hồ sơ",
        _ => s.ToString()
    };

    public static string Vi(JobOrderStatus s) => s switch
    {
        JobOrderStatus.Recruiting => "Đang tuyển",
        JobOrderStatus.FullProfiles => "Đủ hồ sơ",
        JobOrderStatus.Interviewing => "Đang phỏng vấn",
        JobOrderStatus.Closed => "Đã chốt",
        JobOrderStatus.Cancelled => "Đóng đơn",
        _ => s.ToString()
    };

    public static string Vi(WorkflowStepStatus s) => s switch
    {
        WorkflowStepStatus.Pending => "Chờ xử lý",
        WorkflowStepStatus.InProgress => "Đang xử lý",
        WorkflowStepStatus.Completed => "Hoàn thành",
        WorkflowStepStatus.Skipped => "Bỏ qua",
        WorkflowStepStatus.Failed => "Thất bại",
        _ => s.ToString()
    };

    public static string Vi(Gender g) => g switch
    {
        Gender.Male => "Nam",
        Gender.Female => "Nữ",
        Gender.Other => "Khác",
        _ => g.ToString()
    };

    public static string Vi(MaritalStatus m) => m switch
    {
        MaritalStatus.Single => "Độc thân",
        MaritalStatus.Married => "Đã kết hôn",
        MaritalStatus.Divorced => "Ly hôn",
        MaritalStatus.Widowed => "Góa",
        _ => m.ToString()
    };

    public static string Vi(CandidateJobOrderStatus s) => s switch
    {
        CandidateJobOrderStatus.Active => "Đang xử lý",
        CandidateJobOrderStatus.Dropped => "Đã dừng",
        CandidateJobOrderStatus.Completed => "Hoàn tất",
        _ => s.ToString()
    };

    public static string Vi(PaymentType t) => t switch
    {
        PaymentType.Deposit => "Đặt cọc",
        PaymentType.DocumentFee => "Phí hồ sơ",
        PaymentType.TrainingFee => "Phí đào tạo",
        PaymentType.VisaFee => "Phí visa",
        PaymentType.ServiceFee => "Phí dịch vụ",
        PaymentType.OtherIncome => "Thu khác",
        _ => t.ToString()
    };

    public static string Vi(PaymentStatus s) => s switch
    {
        PaymentStatus.Pending => "Chờ thu",
        PaymentStatus.Partial => "Thu một phần",
        PaymentStatus.Paid => "Đã thu",
        PaymentStatus.Overdue => "Quá hạn",
        PaymentStatus.Refunded => "Đã hoàn",
        _ => s.ToString()
    };

    public static Color ColorOf(PaymentStatus s) => s switch
    {
        PaymentStatus.Paid => Color.Success,
        PaymentStatus.Partial => Color.Info,
        PaymentStatus.Pending => Color.Warning,
        PaymentStatus.Overdue => Color.Error,
        PaymentStatus.Refunded => Color.Default,
        _ => Color.Default
    };

    public static string Vi(PaymentMethod m) => m switch
    {
        PaymentMethod.Cash => "Tiền mặt",
        PaymentMethod.BankTransfer => "Chuyển khoản",
        PaymentMethod.Other => "Khác",
        _ => m.ToString()
    };

    public static string Vi(ExpenseCategory c) => c switch
    {
        ExpenseCategory.Marketing => "Marketing",
        ExpenseCategory.Partner => "Đối tác",
        ExpenseCategory.Document => "Hồ sơ",
        ExpenseCategory.Visa => "Visa",
        ExpenseCategory.Training => "Đào tạo",
        ExpenseCategory.Refund => "Hoàn tiền",
        ExpenseCategory.Other => "Khác",
        _ => c.ToString()
    };

    public static string Vi(CommissionMilestone m) => m switch
    {
        CommissionMilestone.Deposit => "Đặt cọc",
        CommissionMilestone.Selected => "Trúng tuyển",
        CommissionMilestone.Departure => "Xuất cảnh",
        _ => m.ToString()
    };

    public static string Vi(CommissionStatus s) => s switch
    {
        CommissionStatus.Pending => "Chờ duyệt",
        CommissionStatus.Approved => "Đã duyệt",
        CommissionStatus.Paid => "Đã chi",
        CommissionStatus.Cancelled => "Đã hủy",
        _ => s.ToString()
    };

    public static Color ColorOf(CommissionStatus s) => s switch
    {
        CommissionStatus.Paid => Color.Success,
        CommissionStatus.Approved => Color.Info,
        CommissionStatus.Pending => Color.Warning,
        CommissionStatus.Cancelled => Color.Error,
        _ => Color.Default
    };

    public static string Vi(VisaStatus s) => s switch
    {
        VisaStatus.NotSubmitted => "Chưa nộp",
        VisaStatus.Preparing => "Đang chuẩn bị",
        VisaStatus.Submitted => "Đã nộp",
        VisaStatus.AdditionalRequired => "Bổ sung hồ sơ",
        VisaStatus.Approved => "Đậu visa",
        VisaStatus.Rejected => "Bị từ chối",
        _ => s.ToString()
    };

    public static Color ColorOf(VisaStatus s) => s switch
    {
        VisaStatus.Approved => Color.Success,
        VisaStatus.Submitted => Color.Info,
        VisaStatus.Preparing or VisaStatus.AdditionalRequired => Color.Warning,
        VisaStatus.Rejected => Color.Error,
        _ => Color.Default
    };
}
