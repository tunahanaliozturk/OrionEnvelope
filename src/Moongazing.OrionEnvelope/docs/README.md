# OrionEnvelope

One HTTP contract for a .NET API: every success is a typed `{ data, meta }` envelope and every failure an RFC 9457 `problem+json` body, projected from OrionResult's `Result<T>` instead of hand-shaped per endpoint.

![How a Result<T> is projected to Envelope<T> or ProblemDetails, and how each ErrorKind maps to an HTTP status](https://raw.githubusercontent.com/tunahanaliozturk/OrionEnvelope/master/docs/diagrams/result-projection.png)

## Install

    dotnet add package OrionEnvelope

Depends on `OrionResult` 0.9.x for `Result<T>`, `Error` and `ErrorKind`.

## Quick start

```csharp
using Moongazing.OrionEnvelope;
using Moongazing.OrionResult;

Result<OrderDto> result = await orders.GetAsync(id, ct);

if (result.IsSuccess)
{
    var meta = new Meta { TraceId = traceId, Page = new PageMeta(nextCursor, hasMore: true) };
    Envelope<OrderDto> ok = result.ToEnvelope(meta);
    // { "data": { ... }, "meta": { "traceId": "...", "page": { "nextCursor": "...", "hasMore": true } } }
}
else
{
    ProblemDetails problem = result.ToProblemDetails(traceId);
    // problem.Status comes from the first error's ErrorKind, e.g. NotFound -> 404
}
```

Serialize through a source-gen context so no reflection is involved (AOT-safe):

```csharp
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                             DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Envelope<OrderDto>))]
[JsonSerializable(typeof(ProblemDetails))]
public partial class ApiJson : JsonSerializerContext;

string json = JsonSerializer.Serialize(ok, ApiJson.Default.EnvelopeOrderDto);
```

## What you get

- `Envelope<T>` with `Data` and `Meta`, plus `Envelope.Ok(data, meta)`.
- `Meta`: `TraceId`, `Page` (`PageMeta(nextCursor, hasMore)`) and `Warnings`; null members are left off the wire.
- `ProblemDetails`: `Type` (default `about:blank`), `Title`, `Status`, `Detail`, `Instance`, plus the extensions `TraceId`, `Code` and per-field `Errors` (`ProblemFieldError`).
- `EnvelopeMapper`: `Result<T>.ToEnvelope(meta)`, `Result<T>.ToProblemDetails(traceId, instance)`, `Error.ToProblemDetails(traceId, instance)` and `StatusFor(ErrorKind)`.
- `OrionEnvelopeJsonContext`: a source-gen context for `Meta`, `PageMeta`, `ProblemDetails` and `ProblemFieldError`. Add your own `Envelope<TDto>` types to your context.
- `AddOrionEnvelope(Action<EnvelopeOptions>?)` registers `EnvelopeOptions` (`WrapSuccessResponses`, `ProblemDetailsForErrors`, `IncludeTraceId`, all `true` by default).

## Behaviour

- `ToEnvelope` on a failed result and `ToProblemDetails` on a successful one throw `InvalidOperationException`.
- A failed result with several errors takes `Status`, `Title`, `Detail` and `Code` from the first error; `Errors` collects the field errors of every error.
- Status map: Validation 400, Unauthorized 401, Forbidden 403, NotFound 404, Conflict 409, FailedPrecondition 422, RateLimited 429, Unexpected 500, Unavailable 503, Timeout 504; any other kind 500.
- Wave 1 scope (0.1): the types and the projection. Nothing reads `EnvelopeOptions` yet; the ASP.NET filters that wrap responses automatically, the exception mapper and the OpenAPI schema come in later releases. Today you call the projection at the boundary.
- AOT- and trim-compatible (`IsAotCompatible`), checked by a NativeAOT smoke test in CI. Targets net8.0, net9.0 and net10.0.

## Related packages

- `OrionResult` - the `Result<T>` and `Error` types this package projects.
- `OrionPage` - keyset pagination whose cursor fits `meta.page`.

## Links

- Documentation and full README: https://github.com/tunahanaliozturk/OrionEnvelope
- Changelog: https://github.com/tunahanaliozturk/OrionEnvelope/blob/master/CHANGELOG.md
- License: MIT
