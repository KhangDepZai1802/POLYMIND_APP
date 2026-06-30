using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Polymind.Domain.Enums;

namespace Polymind.Web.Storage;

public sealed class MinioDocumentStorage(IOptions<MinioStorageOptions> options) : IDocumentStorage
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".webp",
        ".doc", ".docx", ".xls", ".xlsx"
    };

    private readonly MinioStorageOptions _options = options.Value;

    public long MaxUploadBytes => _options.MaxUploadBytes;

    public async Task<UploadedDocumentObject> UploadAsync(
        Guid candidateId,
        DocumentType documentType,
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        var objectKey = string.Join('/',
            "candidates",
            candidateId.ToString("N"),
            documentType.ToString().ToLowerInvariant(),
            BuildStoredFileName(file));

        return await UploadObjectAsync(objectKey, file, cancellationToken);
    }

    public async Task<UploadedDocumentObject> UploadMessageAttachmentAsync(
        Guid senderId,
        Guid recipientId,
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        var objectKey = string.Join('/',
            "messages",
            senderId.ToString("N"),
            recipientId.ToString("N"),
            BuildStoredFileName(file));

        return await UploadObjectAsync(objectKey, file, cancellationToken);
    }

    private async Task<UploadedDocumentObject> UploadObjectAsync(
        string objectKey,
        IBrowserFile file,
        CancellationToken cancellationToken)
    {
        ValidateOptions();
        if (file.Size <= 0)
            throw new InvalidOperationException("File rỗng.");
        if (file.Size > _options.MaxUploadBytes)
            throw new InvalidOperationException($"File vượt quá giới hạn {_options.MaxUploadBytes / 1024 / 1024:N0} MB.");

        var fileName = SanitizeFileName(file.Name);
        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Chỉ hỗ trợ PDF, ảnh, Word và Excel.");

        var client = BuildClient();
        await EnsureBucketAsync(client, cancellationToken);

        await using var stream = file.OpenReadStream(_options.MaxUploadBytes, cancellationToken);
        var args = new PutObjectArgs()
            .WithBucket(_options.Bucket)
            .WithObject(objectKey)
            .WithStreamData(stream)
            .WithObjectSize(file.Size)
            .WithContentType(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);

        await client.PutObjectAsync(args, cancellationToken);
        return new UploadedDocumentObject(objectKey, fileName, file.Size, file.ContentType);
    }

    public async Task<string> GetDownloadUrlAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        ValidateOptions();
        var client = BuildClient();
        await EnsureBucketAsync(client, cancellationToken);

        var args = new PresignedGetObjectArgs()
            .WithBucket(_options.Bucket)
            .WithObject(objectKey)
            .WithExpiry(_options.PresignedUrlExpirySeconds);

        return await client.PresignedGetObjectAsync(args);
    }

    private IMinioClient BuildClient()
        => new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(_options.UseSsl)
            .Build();

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint)
            || string.IsNullOrWhiteSpace(_options.AccessKey)
            || string.IsNullOrWhiteSpace(_options.SecretKey)
            || string.IsNullOrWhiteSpace(_options.Bucket))
        {
            throw new InvalidOperationException("Cấu hình MinIO chưa đầy đủ. Kiểm tra Minio__Endpoint/AccessKey/SecretKey/Bucket.");
        }
    }

    private async Task EnsureBucketAsync(IMinioClient client, CancellationToken cancellationToken)
    {
        var exists = await client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_options.Bucket),
            cancellationToken);

        if (!exists)
        {
            await client.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_options.Bucket),
                cancellationToken);
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "document" : name;
    }

    private static string BuildStoredFileName(IBrowserFile file)
    {
        var fileName = SanitizeFileName(file.Name);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension}";
    }
}
