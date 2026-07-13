using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Infrastructure.Persistence;
using Polymind.Infrastructure.Persistence.Constants;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M08 — Training. Pin hợp đồng Domain mà module Đào tạo phụ thuộc.
/// LƯU Ý PHẠM VI: logic nghiệp vụ M08 (clamp 0..100, scope, week-grouping, authorization)
/// nằm trong razor/Polymind.Web nên KHÔNG unit-test được từ project này (không ref Web) —
/// xem 05-automation-report.md. Các test dưới chỉ chốt bất biến enum/entity: nếu ai đó đổi tên/đảo
/// thứ tự enum (DB lưu TEXT tên member) sẽ vỡ mapping dữ liệu cũ + UI → cần migration.
/// TC_M08_029, TC_M08_030 + bất biến default.
/// </summary>
public class M08_TrainingRulesTests
{
    [Fact] // TC_M08_030 — 2 mảng đào tạo tách biệt (Vietgroup)
    public void TrainingTrack_has_exactly_language_and_vocational()
    {
        Assert.Equal(
            new[] { TrainingTrack.Language, TrainingTrack.Vocational },
            Enum.GetValues<TrainingTrack>());
    }

    [Fact] // TC_M08_029 — thang 4 mức, đúng thứ tự dùng trong RatingSelect
    public void EvaluationRating_has_four_levels_in_ascending_order()
    {
        Assert.Equal(
            new[] { EvaluationRating.Weak, EvaluationRating.Average, EvaluationRating.Good, EvaluationRating.Excellent },
            Enum.GetValues<EvaluationRating>());
    }

    [Fact] // Bất biến default: record mới = "có học mảng này"
    public void New_training_record_defaults_to_enrolled()
    {
        var rec = new TrainingRecord();

        Assert.True(rec.IsEnrolled);
        Assert.Equal(0, rec.ProgressPercent);
    }

    [Fact] // Track của phiếu đánh giá cho phép null = "đánh giá chung"
    public void Training_evaluation_track_is_optional_for_general_review()
    {
        var eval = new TrainingEvaluation { Track = null };

        Assert.Null(eval.Track);
    }

    [Theory] // CR-M08-1 / U-M08-1 — mở quyền chỉ đọc cho các bộ phận liên quan
    [InlineData(RoleNames.Recruiter)]
    [InlineData(RoleNames.DocumentStaff)]
    [InlineData(RoleNames.VisaStaff)]
    [InlineData(RoleNames.Accountant)]
    public void Related_staff_can_read_training_but_cannot_mutate_it(string roleName)
    {
        Assert.True(DbSeeder.RoleHasPermission(roleName, "training:read"));
        Assert.False(DbSeeder.RoleHasPermission(roleName, "training:create"));
        Assert.False(DbSeeder.RoleHasPermission(roleName, "training:update"));
        Assert.False(DbSeeder.RoleHasPermission(roleName, "training:delete"));
        Assert.False(DbSeeder.RoleHasPermission(roleName, "training:approve"));
    }
}
