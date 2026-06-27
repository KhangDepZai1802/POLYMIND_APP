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

/// <summary>Trạng thái đơn hàng tuyển dụng (mục 5.2).</summary>
public enum JobOrderStatus
{
    Recruiting, FullProfiles, Interviewing, Closed, Cancelled
}

/// <summary>17 bước workflow ứng viên (mục 6.1 / docs 02 mục 5). Giá trị = thứ tự bước.</summary>
public enum WorkflowStep
{
    Lead = 1, Consulting = 2, Registration = 3, Deposit = 4, Document = 5,
    HealthCheck = 6, Orientation = 7, EntranceExam = 8, Selected = 9, SignContract = 10,
    VisaSubmit = 11, VisaApproved = 12, FullPayment = 13, BookFlight = 14, Departure = 15,
    Arrived = 16, Completed = 17
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
    ReminderVisa, ReminderDeparture, CommissionPayment
}

public enum NotificationChannel { Email, Sms, Zalo, InApp }

public enum Gender { Male, Female, Other }
