param(
    [string]$ComposeFile = "docker-compose.production.yml",
    [string]$EnvFile = ".env.production",
    [string]$BackupDir = "db-backups"
)

$ErrorActionPreference = "Stop"

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$target = Join-Path $BackupDir $timestamp
New-Item -ItemType Directory -Force -Path $target | Out-Null

$dbFile = Join-Path $target "polymind-db.sql"
$minioFile = Join-Path $target "polymind-minio.tar.gz"

Write-Host "Backing up PostgreSQL to $dbFile"
docker compose --env-file $EnvFile -f $ComposeFile exec -T postgres sh -c 'pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB"' > $dbFile

Write-Host "Backing up MinIO volume to $minioFile"
$remoteMinioBackup = "/tmp/polymind-minio-$timestamp.tar.gz"
docker compose --env-file $EnvFile -f $ComposeFile exec -T minio sh -c "tar -czf $remoteMinioBackup -C /data ."
docker compose --env-file $EnvFile -f $ComposeFile cp "minio:$remoteMinioBackup" $minioFile
docker compose --env-file $EnvFile -f $ComposeFile exec -T minio sh -c "rm -f $remoteMinioBackup"

Write-Host "Backup complete: $target"
