using System.Security.Claims;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Persistence.Constants;

namespace Polymind.Web.Display;

public static class WorkflowStepAccess
{
    public static bool CanAssignJobOrder(ClaimsPrincipal user)
        => user.IsInRole(RoleNames.SuperAdmin) || IsRecruitment(user);

    public static bool CanAdvance(ClaimsPrincipal user, WorkflowStep step)
    {
        if (step >= WorkflowStep.Completed) return false;
        if (user.IsInRole(RoleNames.SuperAdmin)) return true;

        return step switch
        {
            WorkflowStep.Lead or WorkflowStep.Contacted or WorkflowStep.Consulting or WorkflowStep.Registration
                => IsRecruitment(user),

            WorkflowStep.Deposit or WorkflowStep.FullPayment
                => user.IsInRole(RoleNames.Accountant),

            WorkflowStep.Document or WorkflowStep.HealthCheck or WorkflowStep.Orientation
                => user.IsInRole(RoleNames.DocumentStaff),

            // Bước 7.5: gắn lại đơn hàng mới sau khi rớt — cùng nhóm quyền với gắn đơn.
            WorkflowStep.ReselectJobOrder
                => IsRecruitment(user),

            WorkflowStep.EntranceExam or WorkflowStep.Selected
                => user.IsInRole(RoleNames.DocumentStaff) || IsRecruitment(user),

            WorkflowStep.SignContract
                => user.IsInRole(RoleNames.DocumentStaff) || user.IsInRole(RoleNames.Accountant),

            WorkflowStep.CoeApplication or WorkflowStep.VisaSubmit or WorkflowStep.VisaApproved
                => user.IsInRole(RoleNames.VisaStaff),

            WorkflowStep.BookFlight or WorkflowStep.Departure or WorkflowStep.Arrived
                => user.IsInRole(RoleNames.VisaStaff) || IsRecruitment(user),

            WorkflowStep.OverseasSupport
                => IsRecruitment(user) || user.IsInRole(RoleNames.DocumentStaff),

            _ => false
        };
    }

    public static string OwnerLabel(WorkflowStep step) => step switch
    {
        WorkflowStep.Lead or WorkflowStep.Contacted or WorkflowStep.Consulting or WorkflowStep.Registration
            => "Tuyển dụng / Tư vấn viên",
        WorkflowStep.Deposit or WorkflowStep.FullPayment
            => "Kế toán",
        WorkflowStep.Document or WorkflowStep.HealthCheck or WorkflowStep.Orientation
            => "Bộ phận hồ sơ",
        WorkflowStep.ReselectJobOrder
            => "Tuyển dụng / Tư vấn viên",
        WorkflowStep.EntranceExam or WorkflowStep.Selected
            => "Bộ phận hồ sơ + Tuyển dụng",
        WorkflowStep.SignContract
            => "Kế toán + Bộ phận hồ sơ",
        WorkflowStep.CoeApplication or WorkflowStep.VisaSubmit or WorkflowStep.VisaApproved
            => "Bộ phận Visa",
        WorkflowStep.BookFlight or WorkflowStep.Departure or WorkflowStep.Arrived
            => "Bộ phận Visa + Tuyển dụng",
        WorkflowStep.OverseasSupport
            => "Tuyển dụng + Bộ phận hồ sơ",
        WorkflowStep.Completed
            => "Hoàn tất",
        _ => "Chưa xác định"
    };

    private static bool IsRecruitment(ClaimsPrincipal user)
        => user.IsInRole(RoleNames.RecruitmentManager)
           || user.IsInRole(RoleNames.Recruiter)
           || user.IsInRole(RoleNames.Consultant);
}
