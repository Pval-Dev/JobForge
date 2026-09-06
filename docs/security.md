# Security Model

JobForge is secure-by-default at the HTTP boundary, while remaining explicit about the controls that are outside the scope of this portfolio project.

## Authentication

JWT Bearer authentication validates issuer, audience, expiration, signing key, and lifetime with zero clock skew. Tokens include user ID, username, role, and a unique JWT ID.

## Password storage

Passwords are never persisted. `IPasswordHasher<User>` stores only `PasswordHash`. Login intentionally returns the same authentication error for an unknown username and an incorrect password to reduce username-enumeration leakage.

## Authorization

A fallback policy requires authentication for every endpoint unless the endpoint explicitly calls `.AllowAnonymous()`. Admin operations use the `AdminOnly` policy. Resource-specific operations also verify ownership.

## Ownership / BOLA protection

Job and webhook authorization is not based on knowing a GUID. For job creation, `OwnerId` comes from the authenticated JWT identity rather than request JSON. For reads and mutations, the service/endpoint verifies that the caller is the owner or an Admin.

## Response boundaries

The HTTP API does not serialize `User` directly. `UserResponse` excludes `PasswordHash` and other internal state.

## Initial administrator

The first Admin is bootstrapped only if no Admin exists. Credentials are expected through local configuration such as .NET user-secrets.

```text
configure BootstrapAdmin credentials
→ run application once
→ verify Admin login
→ remove BootstrapAdmin credentials
```

The account remains in PostgreSQL because only the password hash is persisted.

## Secret management

The repository intentionally excludes database passwords, JWT signing keys, bootstrap Admin passwords, access tokens, and local `.env` files. `UserSecretsId` in the project file is an identifier, not a secret value.

## Webhook boundary

Webhook registration validates that the URL is absolute and uses HTTP or HTTPS. `IHttpClientFactory` is used for outbound delivery and the named client has a finite timeout.

A production deployment would additionally need SSRF/private-network protection, DNS rebinding defenses, egress policy, payload signatures, delivery idempotency, and durable delivery/dead-letter handling.

## Current security limitations

- Access tokens do not have refresh-token rotation or server-side revocation.
- A role claim can remain valid until the current JWT expires; a new token reflects the new role.
- Rate limiting and lockout policies are not implemented.
- Multi-tenant isolation is represented by owner checks, not a database-level tenant policy.
- HTTP is used for local development; deployment should terminate TLS.

These are documented limitations rather than hidden assumptions.