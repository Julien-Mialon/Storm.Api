# Storm.Api — Framework Roadmap & Ideas

A prioritized list of features that would make Storm.Api more useful for **small teams (1–10 devs)** building APIs. The goal is to reduce boilerplate, cover common needs, and stay out of cloud-vendor lock-in.

Guiding rules when picking ideas from this list:

- No cloud-specific SDKs (Azure / AWS / GCP) unless the feature is genuinely widely used regardless of provider (e.g. S3-compatible storage is fine, CloudWatch-specific is not).
- Every idea should plug into the existing patterns (Actions, `Resolve<T>()`, `ILogService`, `BaseStartup` extension methods, source generators).
- Prefer attributes on actions over middleware, to stay consistent with `[WithAction<T>]`.

---

## 1. Top priorities — must-haves for small teams

These remove repetitive per-endpoint code and cover needs every real-world API hits within the first few weeks.

- [ ] **Validation framework**
      Hook into `BaseAction.ValidateParameter()`. Support either attribute-based (`[Required]`, `[MinLength]`, custom `IValidator<T>`) or a FluentValidation-style fluent API. Surface errors as a structured `Response` payload with `field → message` pairs so clients can render per-field messages. Today every action re-implements its own validation.

- [ ] **Rate limiting**
      `[RateLimit(Requests = 60, Window = "1m", By = RateLimitKey.Ip | Account | Custom)]` on actions. Counter storage pluggable: in-memory by default, Redis when available (already wired). Return `429` with `Retry-After` header.

- [ ] **Idempotency keys**
      `[Idempotent(TTL = "24h")]` attribute on non-GET actions. Client sends `Idempotency-Key` header; response is cached (in Redis) and replayed on retry so payment/order creation doesn't double-execute on flaky networks.

- [ ] **Correlation / trace IDs**
      Auto-generated `X-Request-Id` per request, written into every `ILogService` entry, returned in response headers, propagated across outbound `HttpClient` calls and background job enqueues. Single most impactful debugging feature.

- [ ] **Health-check endpoints**
      `/health`, `/health/ready`, `/health/live` wired into `BaseStartup`. Pluggable `IHealthCheck` contributions (DB reachable, Redis reachable, migrations complete, worker queues draining). Essential for any non-trivial deployment.

- [ ] **In-memory cache abstraction**
      `ICacheService` with TTL + `GetOrSet` semantics. Default in-memory impl, Redis impl, easy swap. Lets services do `Resolve<ICacheService>().GetOrSet("key", 5.Minutes(), loader)` without each one reinventing it.

- [ ] **File storage abstraction**
      `IFileStorageService` with local-disk + S3-compatible (MinIO / R2 / B2) implementations. `Upload`, `Download`, `GetStream`, `GetSignedUrl`, `Delete`. S3-compatible is genuinely multi-vendor so it's not lock-in.

- [ ] **Email templating**
      Scriban or Razor renderer next to `ResendEmailService`; `SendTemplatedAsync("welcome", new { Name = ..., Link = ... })`. Today only raw HTML is supported, which means inline-concatenating HTML in actions. Pair with i18n support below.

- [ ] **Testing helpers**
      `ActionTestHost` that spins up a minimal DI container, lets you execute actions against an in-memory SQLite with fake auth + fake `TimeProvider`, without starting Kestrel. Massive leverage for small teams that skip a full test project today.

- [ ] **CLI scaffolding tool**
      `dotnet storm new action <Name>`, `new migration <Name>`, `new entity <Name>`, `new controller <Name>`. Generates the 3–5 boilerplate files per convention. Replaces the "copy an existing file and rename" workflow.

---

## 2. High value — ship within the first year

- [ ] **Audit logging**
      `[Audited]` attribute on authenticated actions auto-records `{account_id, action, parameter_summary, result, timestamp}` to a dedicated table or log sink. Queryable per-account. Covers most compliance asks without a separate product.

- [ ] **OpenTelemetry traces**
      Spans around each action execution and OrmLite DB call, exported via OTLP (Jaeger / Tempo / Grafana). Works alongside existing `ILogService` rather than replacing it. Correlation ID from §1 becomes the trace ID.

- [ ] **Strongly-typed error-code catalog**
      Source generator scans for `DomainException("SOME_CODE")` call sites and emits a central `ErrorCodes` const class, plus an OpenAPI extension (`x-error-codes`) so clients see the catalog. De-duplicates codes and makes them refactorable.

- [ ] **Transactional outbox pattern**
      Table + worker that publishes events/webhooks reliably *after* the DB transaction commits. Pairs directly with existing `IDatabaseTransaction` — no lost messages on crash.

- [ ] **Webhook delivery infrastructure**
      Outgoing webhook support with HMAC signing, configurable retries (reuse `ExponentialBackOffStrategy`), delivery log, and replay UI. Pairs with the outbox.

- [ ] **Persistent background jobs**
      A Hangfire-lite backed by the existing DB: enqueue-a-job-that-survives-restart, delayed jobs, recurring jobs. Complements the in-memory queues (which are fine for ephemeral work).

- [ ] **Request / response redaction in logs**
      Configurable list of field names (`password`, `token`, `credit_card`, etc.) automatically masked in `HTTP_LOG` entries. Protects secrets from leaking into Elastic.

- [ ] **Password policies + breach check**
      Configurable min-length / complexity, optional HaveIBeenPwned k-anonymity range query before accepting a new password. One line of config, big security win.

- [ ] **2FA / TOTP**
      Pluggable second factor for `BaseLoginAction`; QR-code provisioning helper; backup codes; optional enforcement per account role.

- [ ] **ETag / conditional GET**
      `[ETag]` attribute on GET actions; framework computes hash, returns `304 Not Modified` when client sends a matching `If-None-Match`. Big bandwidth win on list endpoints.

---

## 3. Nice to have — pick as needs arise

- [ ] **Database seeding**
      `ISeedModule` registered the same way as migration modules, runs idempotently per-environment (dev / staging seeds, no-ops in prod).

- [ ] **Schema drift detection**
      On startup, compare OrmLite entity attributes to the actual DB schema and log a warning for any divergence. Catches "we forgot the migration" before it hits prod.

- [ ] **API versioning**
      `[ApiVersion("v1")]` / `[ApiVersion("v2")]` attributes for routing, with per-version OpenAPI docs. Only useful once a public API exists, but cheap to add then.

- [ ] **Dynamic feature flags**
      Upgrade the current static `FeatureFlags` class to support per-user / per-tenant rules and live reload from config without a restart. Keeps small teams from writing their own flag service.

- [ ] **Multi-tenancy**
      Tenant resolver (subdomain / header / claim), per-tenant DB routing hook, tenant-scoped repositories. Adopt only when actually needed — premature multi-tenancy is expensive.

- [ ] **i18n / localization**
      Resource-based translations for error messages, email templates, and validation messages. Pairs with the email templating and validation items.

- [ ] **Login throttling**
      Exponential-backoff delay after repeated failed logins, keyed on `account + IP`. Wraps the generic rate limiter with the specific semantics auth flows need.

- [ ] **CSRF protection**
      Double-submit cookie helper for cookie-based auth flows. JWT-based flows don't need it, but some apps prefer cookies.

- [ ] **CAPTCHA integration**
      hCaptcha / Cloudflare Turnstile verifier usable in any action (e.g. for public signup / contact forms). Pluggable provider interface.

- [ ] **SSE / WebSocket helpers**
      Thin wrappers for Server-Sent Events and WebSocket endpoints that follow the action pattern where feasible (auth, logging, validation still apply).

- [ ] **Retry + circuit breaker helpers**
      For outbound HTTP calls: thin wrappers (or a Polly integration) registered via DI and configured per-client. Combined with the correlation ID for end-to-end tracing.

---

## Notes on scoping

- Items in §1 are the highest-leverage because they affect **every endpoint** the team writes.
- Items in §2 are typically added once a product goes to production and real users / integrations exist.
- Items in §3 depend on the specific product; don't build them speculatively.
- Avoid adding features that solve problems the team hasn't hit yet — this framework's value is that it stays small and opinionated.
