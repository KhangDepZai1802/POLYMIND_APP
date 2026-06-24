namespace Polymind.Web.Storage;

public sealed class MinioStorageOptions
{
    public string Endpoint { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string Bucket { get; set; } = "polymind-documents";
    public bool UseSsl { get; set; }
    public int PresignedUrlExpirySeconds { get; set; } = 900;
    public long MaxUploadBytes { get; set; } = 20 * 1024 * 1024;
}
