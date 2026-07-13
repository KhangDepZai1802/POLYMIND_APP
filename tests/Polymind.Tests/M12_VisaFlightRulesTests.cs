using Polymind.Domain.Entities;
using Polymind.Domain.Enums;
using Polymind.Domain.Visas;
using Xunit;

namespace Polymind.Tests;

/// <summary>
/// M12 — Visa &amp; Flight. Pin hợp đồng Domain mà module + notification/report dựa vào.
/// Attribution được tách qua Domain factory để regression BUG_M12_01/02 không cần Blazor harness.
/// Routing reminder theo HandledBy và xác nhận xuất cảnh (ActualDepartureAt) vẫn nằm trong
/// `Polymind.Web` → cần integration/UI harness. Các test còn lại chốt enum/default/nullable contract.
/// TC_M12_002,008,019,026..030.
/// </summary>
public class M12_VisaFlightRulesTests
{
    [Fact] // BUG_M12_01 / TC_M12_002,019 — handler phải là authenticated actor
    public void New_visa_is_attributed_to_the_authenticated_actor()
    {
        var actorId = Guid.NewGuid();

        var visa = VisaFlightCreationRules.CreateVisa(actorId);

        Assert.Equal(actorId, visa.HandledBy);
    }

    [Fact] // BUG_M12_02 / TC_M12_008 — assignee phải là authenticated actor
    public void New_flight_is_attributed_to_the_authenticated_actor()
    {
        var actorId = Guid.NewGuid();

        var flight = VisaFlightCreationRules.CreateFlight(actorId);

        Assert.Equal(actorId, flight.AssignedTo);
    }

    [Fact] // TC_M12_026 — VisaStatus có đủ vòng đời 6 trạng thái
    public void VisaStatus_contains_full_lifecycle()
    {
        var all = Enum.GetValues<VisaStatus>();

        Assert.Contains(VisaStatus.NotSubmitted, all);
        Assert.Contains(VisaStatus.Preparing, all);
        Assert.Contains(VisaStatus.Submitted, all);
        Assert.Contains(VisaStatus.AdditionalRequired, all);
        Assert.Contains(VisaStatus.Approved, all);
        Assert.Contains(VisaStatus.Rejected, all);
    }

    [Fact] // TC_M12_027 — hồ sơ visa mới mặc định chưa nộp
    public void New_visa_defaults_to_not_submitted()
    {
        var visa = new Visa();

        Assert.Equal(VisaStatus.NotSubmitted, visa.Status);
    }

    [Fact] // TC_M12_028 — HandledBy là tùy chọn (nguồn recipient reminder visa)
    public void New_visa_has_no_handler_by_default()
    {
        var visa = new Visa();

        Assert.Null(visa.HandledBy);
    }

    [Fact] // TC_M12_029 — ActualDepartureAt tùy chọn (đóng/mở departure reminder + report xuất cảnh)
    public void New_flight_has_no_actual_departure_by_default()
    {
        var flight = new Flight();

        Assert.Null(flight.ActualDepartureAt);
    }

    [Fact] // TC_M12_030 — AssignedTo là tùy chọn
    public void New_flight_has_no_assignee_by_default()
    {
        var flight = new Flight();

        Assert.Null(flight.AssignedTo);
    }
}
