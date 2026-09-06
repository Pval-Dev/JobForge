# Testing and Verification

JobForge includes a manual HTTP collection and an automated PowerShell smoke runner.

## Automated E2E runner

The full run at `scripts/e2e.ps1` verifies the application from outside the process over HTTP:

```text
Admin login
→ JWT returned
→ runtime Worker created
→ normal User registered
→ missing JWT returns 401
→ normal User on Admin endpoint returns 403
→ ECHO Job created
→ Job queued
→ Worker claims Job
→ Handler executes
→ Job reaches Succeeded
→ Worker returns Idle
→ FAIL_ALWAYS Job created
→ retry attempts persist
→ Scheduler requeues due retries
→ Job eventually reaches Failed
→ Metrics endpoints respond
→ Audit endpoint contains expected lifecycle events
```

### Run

With the API already running:

```powershell
$env:JOBFORGE_ADMIN_PASSWORD = "YOUR_ADMIN_PASSWORD"
.\scripts\e2e.ps1
Remove-Item Env:JOBFORGE_ADMIN_PASSWORD
```

Skip the deliberately slow retry/backoff section:

```powershell
.\scripts\e2e.ps1 -AdminPassword "YOUR_ADMIN_PASSWORD" -SkipRetry
```

## Expected success flow

```text
Created → Queued → Running → Succeeded
```

Its audit trail should contain at least `JobCreated`, `JobQueued`, `JobStarted`, and `JobSucceeded`.

## Expected retry flow

A failing handler repeatedly moves through `Queued → Running → Retrying → Scheduled retry → Queued` until the retry budget is exhausted, after which it reaches `Failed`.

## Manual HTTP collection

`requests/JobForge.http` contains requests for Admin login, worker registration, user registration, job creation and queueing, metrics, audit, and 401/403 checks.

## CI

`.github/workflows/ci.yml` restores and builds the project in Release mode on pushes and pull requests to `main`. The database-backed E2E runner remains an explicit environment-dependent test because it requires PostgreSQL and local authentication configuration.