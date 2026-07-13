using Polymind.Domain.JobOrders;
using Xunit;

namespace Polymind.Tests;

/// <summary>M06 — BUG_M06_01 / BF-M06-02 / TC_M06_005.</summary>
public class M06_JobOrderCreationRulesTests
{
    [Fact]
    public void New_job_order_is_attributed_to_the_authenticated_actor()
    {
        var actorId = Guid.NewGuid();

        var jobOrder = JobOrderCreationRules.Create(actorId, "JO-TEST-001");

        Assert.Equal(actorId, jobOrder.CreatedBy);
        Assert.Equal("JO-TEST-001", jobOrder.Code);
    }
}
