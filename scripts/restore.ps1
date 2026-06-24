param(
    [Parameter(Mandatory = $true)]
    [string]$BackupDir,
    [string]$ComposeFile = "docker-compose.production.yml",
    [string]$EnvFile = ".env.production",
    [switch]$ConfirmRestore
)

$ErrorActionPreference = "Stop"

if (-not $ConfirmRestore) {
    throw "Restore is destructive. Re-run with -ConfirmRestore after verifying BackupDir."
}

$dbFile = Join-Path $BackupDir "polymind-db.sql"
$minioFile = Join-Path $BackupDir "polymind-minio.tar.gz"

if (-not (Test-Path $dbFile)) { throw "Missing DB backup: $dbFile" }
if (-not (Test-Path $minioFile)) { throw "Missing MinIO backup: $minioFile" }

Write-Host "Restoring PostgreSQL from $dbFile"
docker compose --env-file $EnvFile -f $ComposeFile exec -T postgres sh -c 'psql -U "$POSTGRES_USER" "$POSTGRES_DB" -c "drop schema public cascade; create schema public;"'
Get-Content -Raw $dbFile | docker compose --env-file $EnvFile -f $ComposeFile exec -T postgres sh -c 'psql -U "$POSTGRES_USER" "$POSTGRES_DB"'

Write-Host "Restoring MinIO data from $minioFile"
$remoteMinioBackup = "/tmp/polymind-minio-restore.tar.gz"
docker compose --env-file $EnvFile -f $ComposeFile cp $minioFile "minio:$remoteMinioBackup"
docker compose --env-file $EnvFile -f $ComposeFile exec -T minio sh -c "rm -rf /data/* && tar -xzf $remoteMinioBackup -C /data && rm -f $remoteMinioBackup"

Write-Host "Restore complete. Restarting services..."
docker compose --env-file $EnvFile -f $ComposeFile restart web nginx
