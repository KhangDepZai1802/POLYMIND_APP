using Polymind.Domain.Common;

namespace Polymind.Domain.Entities;

/// <summary>Vé máy bay & xuất cảnh (Module 6 / mục 10).</summary>
public class Flight : BaseEntity
{
    public Guid CandidateId { get; set; }
    public Guid JobOrderId { get; set; }
    public string? Airline { get; set; }
    public string? TicketCode { get; set; }
    public DateOnly? DepartureDate { get; set; }
    public TimeOnly? DepartureTime { get; set; }
    public string? DepartureAirport { get; set; }
    public string? DestinationCountry { get; set; }
    public string? DestinationAirport { get; set; }
    public DateTimeOffset? ActualDepartureAt { get; set; }
    public Guid? AssignedTo { get; set; }
    public string? Notes { get; set; }
}
