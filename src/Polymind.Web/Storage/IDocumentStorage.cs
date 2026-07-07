using Microsoft.AspNetCore.Components.Forms;
using Polymind.Domain.Enums;

namespace Polymind.Web.Storage;

public interface IDocumentStorage
{
    long MaxUploadBytes { get; }

    Task<UploadedDocumentObject> UploadAsync(
        Guid candidateId,
        DocumentType documentType,
        IBrowserFile file,
        CancellationToken cancellationToken = default);

    Task<UploadedDocumentObject> UploadMessageAttachmentAsync(
        Guid senderId,
        Guid recipientId,
        IBrowserFile file,
        CancellationToken cancellationToken = default);

    /// <summary>Tải lên tin nhắn thoại (ghi âm) từ dữ liệu nhị phân do trình duyệt (MediaRecorder) tạo.</summary>
    Task<UploadedDocumentObject> UploadMessageAudioAsync(
        Guid senderId,
        Guid recipientId,
        byte[] data,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default);

    Task<string> GetDownloadUrlAsync(
        string objectKey,
        CancellationToken cancellationToken = default);
}

public sealed record UploadedDocumentObject(
    string ObjectKey,
    string FileName,
    long FileSize,
    string? ContentType);
