using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Polymind.Infrastructure.Persistence;
using Polymind.Web.Storage;

namespace Polymind.Web.Health;

public sealed class DatabaseHealthCheck(IDbContextFactory<ApplicationDbContext> dbFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("PostgreSQL reachable")
                : HealthCheckResult.Unhealthy("PostgreSQL not reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL health check failed", ex);
        }
    }
}

public sealed class MinioHealthCheck(IOptions<MinioStorageOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var opts = options.Value;
        if (string.IsNullOrWhiteSpace(opts.Endpoint)
            || string.IsNullOrWhiteSpace(opts.AccessKey)
            || string.IsNullOrWhiteSpace(opts.SecretKey)
            || string.IsNullOrWhiteSpace(opts.Bucket))
        {
            return HealthCheckResult.Unhealthy("MinIO configuration is incomplete");
        }

        try
        {
            var client = new MinioClient()
                .WithEndpoint(opts.Endpoint)
                .WithCredentials(opts.AccessKey, opts.SecretKey)
                .WithSSL(opts.UseSsl)
                .Build();

            var exists = await client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(opts.Bucket),
                cancellationToken);

            return exists
                ? HealthCheckResult.Healthy("MinIO bucket reachable")
                : HealthCheckResult.Degraded($"MinIO bucket '{opts.Bucket}' does not exist yet");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MinIO health check failed", ex);
        }
    }
}
