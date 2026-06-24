param(
    [string]$BaseUrl = "http://localhost:5177",
    [string]$Password = "Admin@123"
)

$ErrorActionPreference = "Stop"

function Get-WebStatusFromError {
    param($ErrorRecord)

    if ($ErrorRecord.Exception.Response -ne $null) {
        return [int]$ErrorRecord.Exception.Response.StatusCode
    }

    if ($ErrorRecord.FullyQualifiedErrorId -match 'MaximumRedirect' -or
        $ErrorRecord.FullyQualifiedErrorId -match 'MaximumRedirection' -or
        $ErrorRecord.Exception.Message -match 'maximum redirect' -or
        $ErrorRecord.Exception.Message -match 'maximum redirection') {
        return 302
    }

    return $null
}

function New-LoginSession {
    param([string]$Email)

    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $login = Invoke-WebRequest -Uri "$BaseUrl/login" -WebSession $session -UseBasicParsing
    $token = [regex]::Match($login.Content, 'name="__RequestVerificationToken" value="([^"]+)"').Groups[1].Value
    if ([string]::IsNullOrWhiteSpace($token)) { throw "Could not find antiforgery token on login page." }

    $form = @{
        '_handler' = 'login'
        '__RequestVerificationToken' = $token
        'Input.Email' = $Email
        'Input.Password' = $Password
    }

    try {
        Invoke-WebRequest -Uri "$BaseUrl/login" -Method Post -WebSession $session -Body $form -UseBasicParsing -MaximumRedirection 0 | Out-Null
    } catch {
        $status = Get-WebStatusFromError $_
        if ($status -ne 302) { throw }
    }

    return $session
}

function Assert-Page {
    param(
        [Microsoft.PowerShell.Commands.WebRequestSession]$Session,
        [string]$Path,
        [int[]]$AllowedStatus = @(200)
    )

    try {
        $response = Invoke-WebRequest -Uri "$BaseUrl$Path" -WebSession $Session -UseBasicParsing -MaximumRedirection 0
        $status = [int]$response.StatusCode
        $hasError = $response.Content -match 'blazor-error-boundary'
    } catch {
        $response = $_.Exception.Response
        $status = Get-WebStatusFromError $_
        $hasError = $false
    }

    if ($AllowedStatus -notcontains $status) {
        throw "Unexpected status for $Path. Expected $($AllowedStatus -join '/'), got $status."
    }
    if ($hasError) {
        throw "Blazor error boundary found on $Path."
    }
    Write-Host "OK $Path $status"
}

$health = Invoke-WebRequest -Uri "$BaseUrl/health" -UseBasicParsing
if ($health.StatusCode -ne 200) { throw "Health endpoint failed." }
Write-Host "OK /health $($health.StatusCode)"

$admin = New-LoginSession "admin@polymind.local"
@(
    "/", "/leads", "/candidates", "/job-orders", "/finance", "/agents",
    "/visa", "/reports", "/notifications", "/admin", "/hangfire"
) | ForEach-Object { Assert-Page -Session $admin -Path $_ }

$agent = New-LoginSession "agent@polymind.local"
Assert-Page -Session $agent -Path "/my-commissions"
Assert-Page -Session $agent -Path "/" -AllowedStatus @(302, 403)
Assert-Page -Session $agent -Path "/reports" -AllowedStatus @(302, 403)

Write-Host "Smoke test completed."
