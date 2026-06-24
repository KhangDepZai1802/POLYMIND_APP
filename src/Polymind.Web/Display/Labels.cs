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
}
