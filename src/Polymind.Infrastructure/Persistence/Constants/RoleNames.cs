namespace Polymind.Infrastructure.Persistence.Constants;

/// <summary>10 vai trò nghiệp vụ (mục 2 đặc tả + Tư vấn viên + CTV tách riêng).</summary>
public static class RoleNames
{
    public const string SuperAdmin = "super_admin";
    public const string Director = "director";
    public const string RecruitmentManager = "recruitment_manager";
    public const string Recruiter = "recruiter";
    public const string Consultant = "consultant";
    public const string DocumentStaff = "document_staff";
    public const string VisaStaff = "visa_staff";
    public const string Accountant = "accountant";
    public const string Agent = "agent";
    public const string Collaborator = "collaborator";

    public static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>
    {
        [SuperAdmin] = "Quyền cao nhất",
        [Director] = "Giám đốc",
        [RecruitmentManager] = "Trưởng phòng tuyển dụng",
        [Recruiter] = "Nhân viên tuyển dụng",
        [Consultant] = "Tư vấn viên",
        [DocumentStaff] = "Bộ phận hồ sơ",
        [VisaStaff] = "Bộ phận visa",
        [Accountant] = "Kế toán",
        [Agent] = "Đại lý",
        [Collaborator] = "Cộng tác viên",
    };
}
