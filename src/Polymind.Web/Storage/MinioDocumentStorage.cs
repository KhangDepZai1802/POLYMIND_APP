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
        if (file.Size <= 0)
            throw new InvalidOperationException("File rỗng.");
        if (file.Size > _options.MaxUploadBytes)
            throw new InvalidOperationException($"File vượt quá giới hạn {_options.MaxUploadBytes / 1024 / 1024:N0} MB.");

        var fileName = SanitizeFileName(file.Name);
        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Chỉ hỗ trợ PDF, ảnh, Word và Excel.");

        var objectKey = string.Join('/',
            "candidates",
            candidateId.ToString("N"),
            documentType.ToString().ToLowerInvariant(),
            $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}");

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
}
