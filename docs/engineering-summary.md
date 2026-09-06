# Engineering Summary

JobForge is a compact backend system that demonstrates how multiple backend concerns interact rather than presenting each concern as an isolated demo.

## What is being exercised

### Domain modeling
The job lifecycle is an explicit state machine with guarded transitions.

### Persistence
EF Core maps domain state to PostgreSQL, including value conversions and a JSONB `JobResult`.

### Concurrency
Queue claims use a transaction plus `FOR UPDATE SKIP LOCKED` to prevent duplicate processing under concurrent workers.

### Background processing
Worker and scheduler hosted services create scoped runtime work around normal application services.

### Reliability
Retry attempts are persisted, delayed retries reuse the scheduler, and startup recovery handles jobs left in `Running` after interruption.

### Security
JWT authentication, fallback authorization, Admin RBAC, owner checks, password hashing, and response DTOs protect the API boundary.

### Observability
Audit events describe lifecycle changes while metrics summarize durable job state and in-memory worker state.

### External integration
Webhook registrations are persisted and outbound HTTP delivery can be exercised through the test-delivery endpoint.

## Why it is a portfolio project

The main value of JobForge is not the number of endpoints. It is the interaction between:

```text
HTTP authority
+ domain invariants
+ persistence
+ concurrency
+ background execution
+ scheduling/retry
+ authentication/authorization
+ recovery
+ observability
```

That combination is representative of backend systems work and makes the repository useful as a discussion artifact in technical interviews.