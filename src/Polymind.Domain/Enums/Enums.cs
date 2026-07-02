namespace Polymind.Domain.Enums;

/// <summary>Trạng thái vòng đời Lead (mục 3.3 đặc tả).</summary>
public enum LeadStatus
{
    New, NotContacted, Contacted, Interested, Appointment,
    Consulting, Registered, Converted, Unsuitable, Cancelled
}

/// <summary>Nguồn phát sinh Lead (mục 3.1).</summary>
public enum LeadSource
{
    FacebookAds, TiktokAds, GoogleAds, Website, LandingPage,
    Zalo, Hotline, Agent, Referral, Event
}

/// <summary>Loại hoạt động chăm sóc Lead (lead_activities).</summary>
public enum LeadActivityType
{
    Call, Note, Email, Sms, Zalo, Appointment, StatusChange
}

/// <summary>
/// Nhóm việc làm (góp ý Vietgroup): ngoài nước / trong nước / du học.
/// OverseasJob = 0 để các đơn hàng cũ (đều là XKLĐ) mặc định đúng nhóm.
/// </summary>
public enum JobCategory { OverseasJob, DomesticJob, StudyAbroad }

/// <summary>Trạng thái đơn hàng tuyển dụng (mục 5.2).</summary>
public enum JobOrderStatus
{
    Recruiting, FullProfiles, Interviewing, Closed, Cancelled
}

/// <summary>
/// Quy trình 20 bước hồ sơ ứng viên (góp ý Vietgroup 07/2026) + bước phụ 7.5 khi rớt thi tuyển.
/// Giá trị = thứ tự trong luồng; DB lưu dạng TEXT (tên member) nên đổi giá trị số an toàn,
/// nhưng đổi TÊN member phải kèm migration UPDATE dữ liệu cũ.
/// Tên member cũ (17 bước) được giữ nguyên để dữ liệu cũ tự map: Consulting → "Đã tư vấn" (B3),
/// Orientation → "Học tiếng/định hướng/nghề" (B10), Completed → "Hoàn thành quy trình" (B20).
/// </summary>
public enum WorkflowStep
{
    Lead = 1,              // B1  Lead mới
    Contacted = 2,         // B2  Đã liên hệ / Liên hệ lại
    Consulting = 3,        // B3  Đã tư vấn
    Registration = 4,      // B4  Đăng ký
    Deposit = 5,           // B5  Đặt cọc
    Document = 6,          // B6  Hoàn thiện hồ sơ
    HealthCheck = 7,       // B7  Khám sức khỏe
    ReselectJobOrder = 8,  // B7.5 Chọn lại đơn hàng khác — chỉ dùng khi rớt B8; sang B8 phải gắn đơn MỚI
    EntranceExam = 9,      // B8  Thi tuyển / Phỏng vấn / Xét duyệt hồ sơ
    Selected = 10,         // B9  Trúng tuyển
    Orientation = 11,      // B10 Học tiếng / Học định hướng / Học nghề (nếu cần)
    SignContract = 12,     // B11 Ký hợp đồng
    CoeApplication = 13,   // B12 Xin COE (Tư cách lưu trú)
    VisaSubmit = 14,       // B13 Nộp hồ sơ Visa
    VisaApproved = 15,     // B14 Đậu Visa
    FullPayment = 16,      // B15 Thanh toán hoàn tất / Cam kết trả nợ
    BookFlight = 17,       // B16 Đặt vé máy bay
    Departure = 18,        // B17 Xuất cảnh
    Arrived = 19,          // B18 Đến nơi làm việc
    OverseasSupport = 20,  // B19 Tương tác giai đoạn xứ người (nhật ký nhiều năm)
    Completed = 21         // B20 Hoàn thành quy trình (khi hết nghĩa vụ: vay tất toán, hết hỗ trợ)
}

/// <summary>Trạng thái xử lý của một bước workflow.</summary>
public enum WorkflowStepStatus
{
    Pending, InProgress, Completed, Skipped, Failed
}

/// <summary>Trạng thái tiến trình ứng viên trên một đơn hàng.</summary>
public enum CandidateJobOrderStatus { Active, Dropped, Completed }

/// <summary>Loại hồ sơ đính kèm ứng viên (mục 4.2).</summary>
public enum DocumentType
{
    Cccd, Passport, HouseholdBook, BirthCert, Degree, Certificate,
    HealthCheck, Photo, CriminalRecord, Contract, Other
}

public enum MaritalStatus { Single, Married, Divorced, Widowed }

/// <summary>Loại khoản thu từ ứng viên (mục 7.1).</summary>
public enum PaymentType
{
    Deposit, DocumentFee, TrainingFee, VisaFee, ServiceFee, OtherIncome
}

public enum PaymentStatus { Pending, Partial, Paid, Overdue, Refunded }

/// <summary>4 bước đóng tiền của ứng viên theo chi phí đơn hàng (20/30/30/20). Giá trị = thứ tự bước.</summary>
public enum PaymentStage
{
    Deposit = 1,      // Đặt cọc
    ServiceFee = 2,   // Đóng phí dịch vụ
    PreDeparture = 3, // Đóng phí trước xuất cảnh
    Settlement = 4    // Tất toán
}

public enum PaymentMethod { Cash, BankTransfer, Other }

/// <summary>Loại khoản chi (mục 7.2).</summary>
public enum ExpenseCategory
{
    Marketing, Partner, Document, Visa, Training, Refund, Other
}

public enum ReceiptType { Income, Expense }

/// <summary>Mốc tính hoa hồng đại lý (mục 8.3): đặt cọc / trúng tuyển / xuất cảnh.</summary>
public enum CommissionMilestone { Deposit, Selected, Departure }

public enum CommissionStatus { Pending, Approved, Paid, Cancelled }

/// <summary>Trạng thái Visa (mục 9.2).</summary>
public enum VisaStatus
{
    NotSubmitted, Preparing, Submitted, AdditionalRequired, Approved, Rejected
}

/// <summary>Loại thông báo tự động (mục 13).</summary>
public enum NotificationType
{
    ReminderDocument, ReminderPayment, ReminderInterview,
    ReminderVisa, ReminderDeparture, CommissionPayment,
    /// <summary>Lead đứng yên một trạng thái quá ngưỡng giờ cho phép → nhắc người phụ trách chăm sóc (góp ý Vietgroup).</summary>
    ReminderLeadCare
}

public enum NotificationChannel { Email, Sms, Zalo, InApp }

public enum Gender { Male, Female, Other }

/// <summary>Tình trạng vay vốn của ứng viên (module Hỗ trợ vay).</summary>
public enum LoanStatus
{
    NotBorrowed, // Legacy: không dùng trên UI; không có record Loan nghĩa là chưa có khoản vay
    Borrowing,   // Đang vay (đã đăng ký/đang làm thủ tục, chưa giải ngân)
    Disbursed,   // Đã giải ngân
    Settled      // Đã tất toán — điều kiện để hồ sơ được chuyển B20 "Hoàn thành quy trình"
}
