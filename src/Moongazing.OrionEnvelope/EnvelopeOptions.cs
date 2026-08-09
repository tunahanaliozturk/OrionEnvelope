namespace Moongazing.OrionEnvelope;

/// <summary>
/// Configures how responses are shaped. The switches are consumed by the ASP.NET filters in a later
/// wave; the option surface ships now so the contract and DI convention are stable from the start.
/// </summary>
public sealed class EnvelopeOptions
{
    /// <summary>Whether successful responses are wrapped in <see cref="Envelope{T}"/> (<c>{ data, meta }</c>). Defaults to true.</summary>
    public bool WrapSuccessResponses { get; set; } = true;

    /// <summary>Whether failures are rendered as RFC 9457 <see cref="ProblemDetails"/>. Defaults to true.</summary>
    public bool ProblemDetailsForErrors { get; set; } = true;

    /// <summary>Whether to populate <see cref="Meta.TraceId"/> / <see cref="ProblemDetails.TraceId"/>. Defaults to true.</summary>
    public bool IncludeTraceId { get; set; } = true;
}
