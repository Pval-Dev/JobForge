# Design Decisions

This document explains the main tradeoffs behind JobForge.

## PostgreSQL-backed queue instead of an in-memory queue

Queued jobs are represented by durable `Job` rows rather than a process-local collection.

**Why:** a restart should not erase pending work. The queue can be reconstructed from persisted state and existing workers can be replaced without losing jobs.

## `FOR UPDATE SKIP LOCKED` for work claiming

The queue claim is the concurrency-critical operation. JobForge uses PostgreSQL row locking rather than a read-then-update sequence.

**Why:** two workers can observe the queue at the same time. Locking the selected row inside a transaction prevents duplicate ownership while still allowing other workers to claim different rows.

## Workers are runtime-only

`WorkerRegistry` uses a `ConcurrentDictionary<Guid, Worker>` and is intentionally not persisted.

**Why:** a worker models capacity in the current process. Persisting it would create stale workers after a restart. Durable work belongs in PostgreSQL; runtime execution capacity can be rebuilt.

## Job state machine in the domain entity

The `Job` entity validates lifecycle transitions itself.

**Why:** `Queued → Running → Succeeded`, retry transitions, and cancellation rules are business invariants. Keeping them inside the entity reduces the chance that a new endpoint or service bypasses lifecycle rules.

## Scheduler as a background polling service

`SchedulerBackgroundService` polls for due schedules once per second.

**Why:** it is simple, observable, and adequate for the scope of this project. A distributed production deployment would require stronger coordination, scheduler ownership/leader election, and likely async database access.

## Retries reuse scheduling

Retries persist a `RetryAttempt`, compute an exponential delay, then create a normal retry schedule.

**Why:** delayed execution and retry execution are the same underlying problem: enqueue work at a future timestamp.

## Secure-by-default routing

Authorization uses a fallback policy requiring authentication globally.

**Why:** adding a new endpoint should not accidentally make it public. Anonymous access must be intentional through `.AllowAnonymous()`.

## Ownership comes from JWT identity

Creating a job never accepts `OwnerId` from the client. The endpoint derives it from `ClaimsPrincipal`.

**Why:** request data is input, not authority. A client should not be able to create a job on behalf of another user by changing a GUID in JSON.

## DTOs at the HTTP boundary

Endpoints return response records rather than exposing EF entities directly where sensitive state exists.

**Why:** the `User` entity contains `PasswordHash`. A response DTO prevents accidental serialization of internal or sensitive fields.

## Global exception translation

Services throw domain/application exceptions and a global exception handler translates them into HTTP status codes and `ProblemDetails` payloads.

**Why:** this avoids repeated `try/catch` blocks in every endpoint and keeps error behavior consistent.

## Bootstrap administrator through configuration

The first Admin account is created only when no Admin exists and bootstrap credentials are supplied through configuration/user-secrets.

**Why:** an HTTP endpoint such as `/make-me-admin` would create an unnecessary privilege-escalation surface. After the initial account exists, the bootstrap secrets can be removed.

## Full source repository

JobForge is presented as a complete backend source repository rather than curated code samples because its value is the interaction between API, persistence, runtime processing, concurrency, security, and background services.