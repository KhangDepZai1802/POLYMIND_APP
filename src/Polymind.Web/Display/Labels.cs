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

    public static Color ColorOf(JobOrderStatus s) => s switch
    {
        JobOrderStatus.Recruiting => Color.Success,
        JobOrderStatus.FullProfiles => Color.Info,
        JobOrderStatus.Interviewing => Color.Warning,
        JobOrderStatus.Closed => Color.Default,
        JobOrderStatus.Cancelled => Color.Error,
        _ => Color.Default
    };

    /// <summary>Đơn "đang tuyển (mở)" = còn nhận hồ sơ → cần nổi bật trong danh sách.</summary>
    public static bool IsOpen(JobOrderStatus s) =>
        s is JobOrderStatus.Recruiting or JobOrderStatus.FullProfiles or JobOrderStatus.Interviewing;

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

    public static string Vi(DocumentType t) => t switch
    {
        DocumentType.Cccd => "CCCD",
        DocumentType.Passport => "Hộ chiếu",
        DocumentType.HouseholdBook => "Sổ hộ khẩu",
        DocumentType.BirthCert => "Giấy khai sinh",
        DocumentType.Degree => "Bằng cấp",
        DocumentType.Certificate => "Chứng chỉ",
        DocumentType.HealthCheck => "Khám sức khỏe",
        DocumentType.Photo => "Ảnh thẻ",
        DocumentType.CriminalRecord => "Lý lịch tư pháp",
        DocumentType.Contract => "Hợp đồng",
        DocumentType.Other => "Khác",
        _ => t.ToString()
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

    public static string Vi(PaymentStage s) => s switch
    {
        PaymentStage.Deposit => "Đặt cọc",
        PaymentStage.ServiceFee => "Đóng phí dịch vụ",
        PaymentStage.PreDeparture => "Đóng phí trước xuất cảnh",
        PaymentStage.Settlement => "Tất toán",
        _ => s.ToString()
    };

    /// <summary>Nhãn ngắn cho stepper 4 bước.</summary>
    public static string ShortVi(PaymentStage s) => s switch
    {
        PaymentStage.Deposit => "Đặt cọc",
        PaymentStage.ServiceFee => "Phí dịch vụ",
        PaymentStage.PreDeparture => "Phí trước bay",
        PaymentStage.Settlement => "Tất toán",
        _ => s.ToString()
    };

    public static string IconOf(PaymentStage s) => s switch
    {
        PaymentStage.Deposit => Icons.Material.Filled.Savings,
        PaymentStage.ServiceFee => Icons.Material.Filled.Handshake,
        PaymentStage.PreDeparture => Icons.Material.Filled.FlightTakeoff,
        PaymentStage.Settlement => Icons.Material.Filled.TaskAlt,
        _ => Icons.Material.Filled.Payments
    };

    /// <summary>Loại khoản thu tương ứng khi tạo Payment cho từng bước.</summary>
    public static PaymentType TypeOf(PaymentStage s) => s switch
    {
        PaymentStage.Deposit => PaymentType.Deposit,
        PaymentStage.ServiceFee => PaymentType.ServiceFee,
        PaymentStage.PreDeparture => PaymentType.TrainingFee,
        PaymentStage.Settlement => PaymentType.OtherIncome,
        _ => PaymentType.OtherIncome
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

    public static string Vi(ReceiptType t) => t switch
    {
        ReceiptType.Income => "Phiếu thu",
        ReceiptType.Expense => "Phiếu chi",
        _ => t.ToString()
    };

    public static Color ColorOf(ReceiptType t) => t switch
    {
        ReceiptType.Income => Color.Success,
        ReceiptType.Expense => Color.Error,
        _ => Color.Default
    };

    public static string Vi(NotificationType t) => t switch
    {
        NotificationType.ReminderDocument => "Nhắc hồ sơ",
        NotificationType.ReminderPayment => "Nhắc khoản thu",
        NotificationType.ReminderInterview => "Nhắc phỏng vấn",
        NotificationType.ReminderVisa => "Nhắc visa",
        NotificationType.ReminderDeparture => "Nhắc xuất cảnh",
        NotificationType.CommissionPayment => "Chi hoa hồng",
        _ => t.ToString()
    };

    public static string IconOf(NotificationType t) => t switch
    {
        NotificationType.ReminderDocument => Icons.Material.Filled.FolderShared,
        NotificationType.ReminderPayment => Icons.Material.Filled.Payments,
        NotificationType.ReminderInterview => Icons.Material.Filled.RecordVoiceOver,
        NotificationType.ReminderVisa => Icons.Material.Filled.Approval,
        NotificationType.ReminderDeparture => Icons.Material.Filled.FlightTakeoff,
        NotificationType.CommissionPayment => Icons.Material.Filled.Handshake,
        _ => Icons.Material.Filled.Notifications
    };

    public static Color ColorOf(NotificationType t) => t switch
    {
        NotificationType.ReminderPayment => Color.Error,
        NotificationType.ReminderVisa or NotificationType.ReminderInterview => Color.Info,
        NotificationType.ReminderDeparture => Color.Success,
        NotificationType.ReminderDocument => Color.Warning,
        NotificationType.CommissionPayment => Color.Secondary,
        _ => Color.Default
    };
}
