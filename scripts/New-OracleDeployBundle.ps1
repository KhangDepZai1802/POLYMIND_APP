[CmdletBinding()]
param(
    [string]$OutputPath = "artifacts/polymind-oracle-deploy.tar.gz"
)

$ErrorActionPreference = "Stop"
$workspace = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$output = if ([System.IO.Path]::IsPathRooted($OutputPath)) {
    [System.IO.Path]::GetFullPath($OutputPath)
} else {
    [System.IO.Path]::GetFullPath((Join-Path $workspace $OutputPath))
}

$workspacePrefix = $workspace.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar
) + [System.IO.Path]::DirectorySeparatorChar
if (-not $output.StartsWith($workspacePrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputPath phải nằm trong workspace: $workspace"
}

$outputDirectory = Split-Path -Parent $output
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

$items = @(
    ".dockerignore",
    ".env.production.example",
    "Dockerfile",
    "Polymind.slnx",
    "docker-compose.production.yml",
    "src",
    "deploy/caddy",
    "scripts/oracle-bootstrap.sh",
    "scripts/init-production-env.sh",
    "scripts/deploy-oracle.sh",
    "scripts/backup.sh",
    "scripts/restore.sh"
)

Push-Location $workspace
try {
    & tar.exe -czf $output `
        --exclude="**/bin" `
        --exclude="**/obj" `
        --exclude="**/logs" `
        --exclude="**/tmppolymind-*" `
        @items
    if ($LASTEXITCODE -ne 0) {
        throw "tar.exe thất bại với exit code $LASTEXITCODE"
    }
} finally {
    Pop-Location
}

$file = Get-Item -LiteralPath $output
Write-Host ("Đã tạo bundle: {0} ({1:N1} MB)" -f $file.FullName, ($file.Length / 1MB))
