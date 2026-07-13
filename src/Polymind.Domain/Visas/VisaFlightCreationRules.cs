using Polymind.Domain.Entities;

namespace Polymind.Domain.Visas;

/// <summary>Tạo hồ sơ visa/vé với người phụ trách là actor đang thao tác.</summary>
public static class VisaFlightCreationRules
{
    public static Visa CreateVisa(Guid actorId)
        => new() { HandledBy = actorId };

    public static Flight CreateFlight(Guid actorId)
        => new() { AssignedTo = actorId };
}
