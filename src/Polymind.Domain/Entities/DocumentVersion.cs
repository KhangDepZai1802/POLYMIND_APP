using Polymind.Domain.Common;

namespace Polymind.Domain.Entities;

/// <summary>Phiên bản hồ sơ (versioning — mục 4.3). File lưu trên MinIO/S3.</summary>
public class DocumentVersion : BaseEntity
{
    public Guid DocumentId { get; set; }
    public CandidateDocument Document { get; set; } = default!;
    public int VersionNumber { get; set; }
    public string FileUrl { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public Guid UploadedBy { get; set; }
    public string? Notes { get; set; }
}
