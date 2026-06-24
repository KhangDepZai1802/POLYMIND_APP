using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Một loại hồ sơ của ứng viên; trỏ tới phiên bản hiện hành.</summary>
public class CandidateDocument : BaseEntity
{
    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = default!;
    public DocumentType DocType { get; set; }
    public Guid? CurrentVersionId { get; set; }

    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}
