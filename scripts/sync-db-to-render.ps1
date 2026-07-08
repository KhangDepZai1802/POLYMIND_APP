<#
  sync-db-to-render.ps1
  Sao chép TOÀN BỘ dữ liệu DB Postgres LOCAL (docker container polymind-postgres) -> DB trên Render.
  Dùng khi cần đồng bộ tài khoản / ứng viên / ... từ máy local lên bản deploy Render.

  CÁCH LẤY CHUỖI KẾT NỐI RENDER:
    Render Dashboard -> mở instance Postgres của app -> mục "Connections"
    -> copy "External Database URL"  (dạng: postgresql://USER:PASS@HOST.oregon-postgres.render.com/DBNAME)

  CÁCH CHẠY (PowerShell, tại thư mục gốc repo):
    ./scripts/sync-db-to-render.ps1 -RenderDbUrl "postgresql://USER:PASS@HOST.oregon-postgres.render.com/DBNAME"

  LƯU Ý QUAN TRỌNG:
    - Ghi ĐÈ toàn bộ dữ liệu trên Render bằng dữ liệu local (DROP + CREATE + COPY từng bảng).
    - Nên chạy lúc KHÔNG có ai đang dùng bản Render; sau khi xong hãy RESTART service Render (Hangfire/app nạp lại).
    - Mật khẩu đăng nhập trên Render sẽ = mật khẩu local (VD admin@polymind.local / Admin@123).
    - File tài liệu/ảnh (MinIO) KHÔNG được copy — chỉ dữ liệu trong DB.
    - Cần: Docker Desktop đang chạy (container polymind-postgres) + psql trên PATH (đã có PostgreSQL client).
#>
param(
  # Không truyền thì tự đọc từ scripts/.render-db-url.txt (file bí mật, đã gitignore).
  [string]$RenderDbUrl,
  [string]$Container = "polymind-postgres",
  [string]$LocalUser = "polymind",
  [string]$LocalDb   = "polymind"
)

$ErrorActionPreference = "Stop"
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$dump  = Join-Path $env:TEMP "polymind-local-$stamp.sql"

# Nguồn URL: tham số -RenderDbUrl > file scripts/.render-db-url.txt.
if ([string]::IsNullOrWhiteSpace($RenderDbUrl)) {
  $urlFile = Join-Path $PSScriptRoot ".render-db-url.txt"
  if (Test-Path $urlFile) {
    $RenderDbUrl = (Get-Content -Raw $urlFile).Trim()
  }
}
if ([string]::IsNullOrWhiteSpace($RenderDbUrl)) {
  throw "Thiếu External Database URL. Truyền -RenderDbUrl `"postgresql://...`" hoặc lưu vào scripts/.render-db-url.txt"
}

Write-Host "==> [1/4] Kiểm tra container '$Container'..." -ForegroundColor Cyan
docker inspect $Container *> $null
if ($LASTEXITCODE -ne 0) { throw "Container '$Container' không chạy. Hãy 'docker compose up -d' trước." }

Write-Host "==> [2/4] Dump DB local (trong container, rồi copy ra ngoài để tránh lỗi mã hoá)..." -ForegroundColor Cyan
# Loại schema 'hangfire' (job nền tự sinh trên Render) để không đụng scheduler đang chạy; mọi dữ liệu ứng dụng vẫn được copy.
docker exec $Container sh -c "pg_dump -U $LocalUser -d $LocalDb --no-owner --no-privileges --no-acl --clean --if-exists --exclude-schema=hangfire -f /tmp/polymind-sync.sql"
if ($LASTEXITCODE -ne 0) { throw "pg_dump trong container thất bại." }
docker cp "$($Container):/tmp/polymind-sync.sql" $dump
docker exec $Container rm -f /tmp/polymind-sync.sql | Out-Null
Write-Host ("    dump: {0}  ({1:N1} MB)" -f $dump, ((Get-Item $dump).Length / 1MB))

Write-Host "==> [3/4] Khôi phục vào Render (SSL bắt buộc, 1 transaction — lỗi thì tự rollback). Có thể mất 1-2 phút..." -ForegroundColor Cyan
$env:PGSSLMODE = "require"
$env:PGOPTIONS = "-c lock_timeout=30000"
psql "$RenderDbUrl" -v ON_ERROR_STOP=1 --single-transaction -f "$dump"
if ($LASTEXITCODE -ne 0) { throw "psql restore vào Render thất bại (đã rollback, DB Render giữ nguyên). Xem lỗi phía trên." }

Write-Host "==> [4/4] Kiểm tra số tài khoản trên Render sau khi copy:" -ForegroundColor Cyan
psql "$RenderDbUrl" -t -c "select count(*) from users;"

Remove-Item $dump -ErrorAction SilentlyContinue
Write-Host ""
Write-Host "XONG. Vào Render RESTART service để app/Hangfire nạp lại. Đăng nhập bằng tài khoản local." -ForegroundColor Green
