# POLYMIND — bật demo cho đối tác qua Cloudflare Tunnel (laptop + tunnel).
# Chạy: chuột phải file -> Run with PowerShell, hoặc: powershell -ExecutionPolicy Bypass -File scripts\demo-start.ps1
# Tự bật Docker + app (cửa sổ riêng) + Cloudflare tunnel, rồi IN + COPY link cho đối tác.

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$tunnelOut = Join-Path $env:TEMP 'polymind-tunnel.out.log'
$tunnelErr = Join-Path $env:TEMP 'polymind-tunnel.err.log'

# Tìm cloudflared
$cf = @(
  'C:\Program Files (x86)\cloudflared\cloudflared.exe',
  'C:\Program Files\cloudflared\cloudflared.exe'
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $cf) { Write-Host 'KHONG tim thay cloudflared. Cai lai: winget install Cloudflare.cloudflared' -ForegroundColor Red; exit 1 }

Write-Host '==> POLYMIND demo launcher' -ForegroundColor Cyan

# 1) Docker
Write-Host '[1/3] Kiem tra Docker...'
docker info *> $null
if (-not $?) {
  Write-Host '      Docker chua chay -> mo Docker Desktop, doi engine (toi 2 phut)...'
  $dd = 'C:\Program Files\Docker\Docker\Docker Desktop.exe'
  if (Test-Path $dd) { Start-Process $dd }
  for ($i=0; $i -lt 40; $i++) { docker info *> $null; if ($?) { break }; Start-Sleep 3 }
}
docker compose -f (Join-Path $root 'docker-compose.yml') up -d | Out-Null
Write-Host '      Docker OK.'

# 2) App
if (Get-NetTCPConnection -LocalPort 5177 -State Listen -ErrorAction SilentlyContinue) {
  Write-Host '[2/3] App da chay san tren cong 5177.'
} else {
  Write-Host '[2/3] Khoi dong app voi Hot Reload (mo cua so rieng)...'
  # dotnet watch: sua code + Ctrl+S la tu ap dung; doi tac chi can F5 refresh.
  # DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true: thay doi lon thi tu restart, khong hoi y/n.
  $cmd = "`$env:ASPNETCORE_ENVIRONMENT='Development'; `$env:ASPNETCORE_URLS='http://0.0.0.0:5177'; `$env:DOTNET_WATCH_RESTART_ON_RUDE_EDIT='true'; dotnet watch --project `"$root\src\Polymind.Web`" run --no-launch-profile"
  Start-Process powershell -ArgumentList '-NoExit','-Command',$cmd
  Write-Host '      Dang build + khoi dong (lan dau hoi lau)...'
  for ($i=0; $i -lt 80; $i++) { if (Get-NetTCPConnection -LocalPort 5177 -State Listen -ErrorAction SilentlyContinue) { break }; Start-Sleep 3 }
}

# 3) Tunnel
if (Get-Process cloudflared -ErrorAction SilentlyContinue) {
  Write-Host '[3/3] Tunnel da chay san.'
} else {
  Write-Host '[3/3] Mo Cloudflare tunnel...'
  Remove-Item $tunnelOut,$tunnelErr -ErrorAction SilentlyContinue
  Start-Process $cf -ArgumentList 'tunnel','--url','http://localhost:5177' `
    -RedirectStandardOutput $tunnelOut -RedirectStandardError $tunnelErr -WindowStyle Hidden
}

# Lay URL tu log
$url = $null
for ($i=0; $i -lt 25; $i++) {
  $m = Get-ChildItem $tunnelOut,$tunnelErr -ErrorAction SilentlyContinue |
       Select-String -Pattern 'https://[a-z0-9-]+\.trycloudflare\.com' -ErrorAction SilentlyContinue |
       Select-Object -First 1
  if ($m) { $url = $m.Matches[0].Value; break }
  Start-Sleep 2
}

Write-Host ''
if ($url) {
  Write-Host "  ===> LINK CHO DOI TAC: $url" -ForegroundColor Green
  try { Set-Clipboard $url; Write-Host '       (Da copy vao clipboard - chi viec dan gui doi tac.)' } catch {}
} else {
  Write-Host '  Chua lay duoc link. Doi them vai giay roi xem file:' -ForegroundColor Yellow
  Write-Host "  $tunnelErr"
}
Write-Host '  Dang nhap demo: admin@polymind.local / Admin@123'
Write-Host '  Tat tat ca: chay scripts\demo-stop.ps1'
