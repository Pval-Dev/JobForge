# API Reference

JobForge uses ASP.NET Core Minimal APIs. Authentication is JWT Bearer unless a route is explicitly anonymous.

## Authentication

| Method | Route | Access | Purpose |
|---|---|---|---|
| `POST` | `/auth/register` | Anonymous | Register a normal user and return an access token |
| `POST` | `/auth/login` | Anonymous | Verify credentials and return an access token |

## Users

| Method | Route | Access | Purpose |
|---|---|---|---|
| `GET` | `/users/me` | Authenticated | Read the current user's safe profile DTO |
| `GET` | `/users/{id}` | Admin | Read another user |
| `PATCH` | `/users/{id}/activate` | Admin | Activate a user |
| `PATCH` | `/users/{id}/deactivate` | Admin | Deactivate a user |
| `PATCH` | `/users/{id}/role` | Admin | Change a user's role |

## Jobs

| Method | Route | Access | Purpose |
|---|---|---|---|
| `POST` | `/jobs` | Authenticated | Create a job owned by the authenticated user |
| `GET` | `/jobs` | Authenticated | List own jobs; Admin sees all jobs |
| `GET` | `/jobs/{id}` | Owner/Admin | Read one job |
| `POST` | `/jobs/{id}/queue` | Owner/Admin | Enqueue a job for runtime processing |
| `POST` | `/jobs/{id}/schedule` | Owner/Admin | Schedule a delayed job for a future time |
| `DELETE` | `/jobs/{id}` | Owner/Admin | Cancel a cancellable job |

### Create job request

```json
{
  "code": "ECHO",
  "name": "Example Job",
  "payload": "{\"message\":\"hello\"}",
  "priority": 1
}
```

The owner ID is intentionally not accepted from the request body. It is derived from the authenticated identity.

## Workers

Workers are runtime-only and all worker management routes are Admin-only.

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/workers` | Register a runtime worker |
| `GET` | `/workers` | List runtime workers |
| `GET` | `/workers/{id}` | Read a worker |
| `PATCH` | `/workers/{id}/online` | Bring an offline worker online |
| `PATCH` | `/workers/{id}/offline` | Take an idle worker offline |

## Metrics

Admin-only: `GET /metrics/jobs`, `GET /metrics/workers`.

## Audit

Admin-only: `GET /audit`, `GET /audit/entity/{id}`, `GET /audit/user/{id}`.

## Webhooks

| Method | Route | Access | Purpose |
|---|---|---|---|
| `POST` | `/webhooks` | Authenticated | Register an owner-scoped webhook |
| `GET` | `/webhooks` | Authenticated | List current user's webhooks |
| `GET` | `/webhooks/all` | Admin | List all registrations |
| `PATCH` | `/webhooks/{id}/activate` | Owner/Admin | Activate registration |
| `PATCH` | `/webhooks/{id}/deactivate` | Owner/Admin | Deactivate registration |
| `POST` | `/webhooks/{id}/test` | Owner/Admin | Send an outbound HTTP test delivery |

## HTTP error mapping

| Exception | HTTP status |
|---|---:|
| `AuthenticationException` | `401 Unauthorized` |
| `UnauthorizedAccessException` | `403 Forbidden` |
| `KeyNotFoundException` | `404 Not Found` |
| `ArgumentException` | `400 Bad Request` |
| `InvalidOperationException` | `409 Conflict` |
| `HttpRequestException` | `502 Bad Gateway` |
| Unexpected exception | `500 Internal Server Error` |

In non-development environments, unexpected server errors return a generic detail message rather than leaking the exception message.