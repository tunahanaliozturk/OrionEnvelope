<!-- markdownlint-disable MD024 -->

# Changelog

All notable changes to OrionEnvelope are documented in this file. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-07-29

The first release — the Orion family's Wave 1 HTTP contract: one success envelope, one RFC 9457
failure shape, projected from `OrionResult`.

### Added

- **`Envelope<T>`** — the success shape `{ data, meta }`; one envelope for every successful response.
- **`Meta`** / **`PageMeta`** — response metadata (`traceId`, `page` = `{ nextCursor, hasMore }`,
  `warnings`), each omitted from the wire when null.
- **`ProblemDetails`** / **`ProblemFieldError`** — an RFC 9457 `application/problem+json` body with the
  family extensions `traceId`, `code`, and per-field `errors`.
- **`EnvelopeMapper`** — the mechanical projection from `OrionResult`:
  - `Result<T>.ToEnvelope(meta)` → `Envelope<T>` on success.
  - `Result<T>.ToProblemDetails(traceId, instance)` / `Error.ToProblemDetails(...)` on failure,
    aggregating validation field errors.
  - `StatusFor(ErrorKind)` — the family's `ErrorKind` → HTTP status map (validation→400, not-found→404,
    conflict→409, failed-precondition→422, unauthorized→401, forbidden→403, rate-limited→429,
    timeout→504, unavailable→503, unexpected→500).
- **`OrionEnvelopeJsonContext`** — a `System.Text.Json` source-gen context for the non-generic wire
  types, so they serialize reflection-free (camelCase, null members omitted). Consumers add their own
  `[JsonSerializable(typeof(Envelope<TDto>))]` for the generic payload.
- **`EnvelopeOptions`** + **`AddOrionEnvelope`** — the option surface (`WrapSuccessResponses`,
  `ProblemDetailsForErrors`, `IncludeTraceId`) and DI convention, stable from the start.
- Binds to `OrionResult` 0.9.0. Multi-targets `net8.0`/`net9.0`/`net10.0`; `IsAotCompatible`; a
  NativeAOT publish smoke test in CI.

### Scope

Wave 1 is the types and the projection. Deliberately deferred: the ASP.NET result filters +
exception→`problem+json` mapper with a stable error-type-URI taxonomy and production-safe detail
gating (W2), the minimal-API/MVC auto-wrapping filters + OpenAPI schema + automatic `OrionPage`
`Page<T>` → `meta.page` folding (W3, GA), and content negotiation + versioned envelopes (W4). The
`meta.page` shape ships now so the contract is complete; the automatic `Page<T>` projection is W3.

### Verified

- Exit criteria met: `Envelope<T>` and `ProblemDetails` round-trip under a source-gen context with
  **no reflection fallback** (verified by an AOT-publish smoke test that publishes trim/AOT-clean under
  `-warnaserror` and exits 0), and golden-file tests pin the exact success and paged-meta JSON shape.
  22 tests green across `net8.0`/`net9.0`/`net10.0`.
