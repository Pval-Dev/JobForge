param(
    [string]$BaseUrl = "http://localhost:5177",
    [string]$AdminPassword = $env:JOBFORGE_ADMIN_PASSWORD,
    [int]$EchoTimeoutSeconds = 15,
    [int]$RetryTimeoutSeconds = 210,
    [switch]$SkipRetry
)

$ErrorActionPreference = "Stop"

function Headers([string]$Token) { @{ Authorization = "Bearer $Token" } }

function Api([string]$Method, [string]$Uri, [hashtable]$Headers, $Body) {
    $p = @{ Uri = $Uri; Method = $Method; ErrorAction = "Stop" }
    if ($Headers) { $p.Headers = $Headers }
    if ($null -ne $Body) {
        $p.ContentType = "application/json"
        $p.Body = $Body | ConvertTo-Json -Depth 10
    }
    Invoke-RestMethod @p
}

function Expect-Status([string]$Method, [string]$Uri, [hashtable]$Headers, [int]$Expected) {
    try {
        $null = Api $Method $Uri $Headers $null
        throw "Expected HTTP $Expected but request succeeded."
    }
    catch {
        if ($_.Exception.Response -and [int]$_.Exception.Response.StatusCode -eq $Expected) {
            Write-Host "[PASS] HTTP $Expected $Method $Uri" -ForegroundColor Green
            return
        }
        throw
    }
}

function Wait-Job([Guid]$Id, [string]$Token, [int]$Timeout, [int[]]$Terminal) {
    $deadline = (Get-Date).AddSeconds($Timeout)
    $last = $null
    while ((Get-Date) -lt $deadline) {
        $job = Api "GET" "$BaseUrl/jobs/$Id" (Headers $Token) $null
        if ($job.status -ne $last) {
            Write-Host "    Job $Id status -> $($job.status)"
            $last = $job.status
        }
        if ($Terminal -contains [int]$job.status) { return $job }
        Start-Sleep -Milliseconds 500
    }
    throw "Timed out waiting for job $Id. Last status: $last"
}

if ([string]::IsNullOrWhiteSpace($AdminPassword)) {
    $AdminPassword = Read-Host "JobForgeAdmin password"
}

Write-Host "`nJobForge automated E2E smoke test" -ForegroundColor Yellow

$admin = Api "POST" "$BaseUrl/auth/login" $null @{ username = "JobForgeAdmin"; password = $AdminPassword }
if ([string]::IsNullOrWhiteSpace($admin.accessToken)) { throw "Admin login returned no token." }
Write-Host "[PASS] Admin authentication" -ForegroundColor Green

$worker = Api "POST" "$BaseUrl/workers" (Headers $admin.accessToken) @{ name = "E2E-Worker-$(Get-Date -Format yyyyMMddHHmmss)" }
Write-Host "[PASS] Worker created" -ForegroundColor Green

$user = Api "POST" "$BaseUrl/auth/register" $null @{ username = "e2e-user-$(Get-Date -Format yyyyMMddHHmmssfff)"; password = "E2E-Test-Password-2026!" }
$userToken = $user.accessToken
Write-Host "[PASS] User registration" -ForegroundColor Green

Expect-Status "GET" "$BaseUrl/users/me" $null 401
Expect-Status "GET" "$BaseUrl/metrics/jobs" (Headers $userToken) 403

$echo = Api "POST" "$BaseUrl/jobs" (Headers $userToken) @{ code = "ECHO"; name = "Automated E2E Echo Job"; payload = '{"message":"Hello from automated JobForge E2E"}'; priority = 1 }
$null = Api "POST" "$BaseUrl/jobs/$($echo.id)/queue" (Headers $userToken) $null
$echoResult = Wait-Job $echo.id $userToken $EchoTimeoutSeconds @(5,6,7)
if ([int]$echoResult.status -ne 5 -or -not $echoResult.result.success) { throw "ECHO job failed." }
Write-Host "[PASS] Queue + ECHO execution" -ForegroundColor Green

$workers = @(Api "GET" "$BaseUrl/workers" (Headers $admin.accessToken) $null)
$runtimeWorker = $workers | Where-Object { $_.id -eq $worker.id } | Select-Object -First 1
if ($null -eq $runtimeWorker -or [int]$runtimeWorker.status -ne 0) { throw "Worker did not return to Idle." }
Write-Host "[PASS] Worker returned to Idle" -ForegroundColor Green

if (-not $SkipRetry) {
    $fail = Api "POST" "$BaseUrl/jobs" (Headers $userToken) @{ code = "FAIL_ALWAYS"; name = "Automated Retry Test"; payload = "{}"; priority = 1 }
    $null = Api "POST" "$BaseUrl/jobs/$($fail.id)/queue" (Headers $userToken) $null
    $failResult = Wait-Job $fail.id $userToken $RetryTimeoutSeconds @(6,7)
    if ([int]$failResult.status -ne 6) { throw "Retry test did not end in Failed." }
    Write-Host "[PASS] Retry + Scheduler" -ForegroundColor Green
}

$null = Api "GET" "$BaseUrl/metrics/jobs" (Headers $admin.accessToken) $null
$null = Api "GET" "$BaseUrl/metrics/workers" (Headers $admin.accessToken) $null
Write-Host "[PASS] Metrics" -ForegroundColor Green

$audit = @(Api "GET" "$BaseUrl/audit/entity/$($echo.id)" (Headers $admin.accessToken) $null)
if ($audit.Count -lt 4) { throw "Expected at least 4 ECHO audit events; found $($audit.Count)." }
Write-Host "[PASS] Audit" -ForegroundColor Green

Write-Host "`nJOBFORGE E2E PASSED" -ForegroundColor Green
