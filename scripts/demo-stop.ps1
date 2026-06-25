# POLYMIND — tắt demo: dừng Cloudflare tunnel + app. (Docker giữ nguyên.)
# Chạy: powershell -ExecutionPolicy Bypass -File scripts\demo-stop.ps1

Write-Host '==> Tat demo POLYMIND...'

# Tunnel
$cf = Get-Process cloudflared -ErrorAction SilentlyContinue
if ($cf) { $cf | Stop-Process -Force; Write-Host '   - Da dung Cloudflare tunnel.' } else { Write-Host '   - Tunnel khong chay.' }

# App (process dang nghe cong 5177)
$conn = Get-NetTCPConnection -LocalPort 5177 -State Listen -ErrorAction SilentlyContinue
if ($conn) {
  $conn.OwningProcess | Select-Object -Unique | ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }
  Write-Host '   - Da dung app (cong 5177).'
} else { Write-Host '   - App khong chay.' }

Write-Host '   - Docker van chay (Postgres/Redis/MinIO). Muon tat han: docker compose down'
Write-Host 'Xong.'
