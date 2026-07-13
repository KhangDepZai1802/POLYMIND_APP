using Polymind.Domain.Entities;

namespace Polymind.Domain.JobOrders;

/// <summary>Tạo JobOrder với attribution bắt buộc từ actor đang thao tác.</summary>
public static class JobOrderCreationRules
{
    public static JobOrder Create(Guid actorId, string code)
        => new()
        {
            Code = code,
            CreatedBy = actorId,
        };
}
