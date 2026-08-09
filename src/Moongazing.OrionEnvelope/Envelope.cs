namespace Moongazing.OrionEnvelope;

using System.Text.Json.Serialization;

/// <summary>
/// The success envelope: a typed <see cref="Data"/> payload plus optional <see cref="Meta"/>. Every
/// successful response across the API shares this one shape, so a client unwraps <c>data</c>/<c>meta</c>
/// once instead of learning a per-endpoint convention. Failures are not enveloped — they are RFC 9457
/// <see cref="ProblemDetails"/> — so the two shapes never overlap.
/// </summary>
/// <typeparam name="T">The payload type.</typeparam>
public sealed record Envelope<T>
{
    /// <summary>Create an envelope.</summary>
    /// <param name="data">The payload.</param>
    /// <param name="meta">Optional metadata (trace id, pagination, warnings).</param>
    public Envelope(T? data, Meta? meta = null)
    {
        Data = data;
        Meta = meta;
    }

    /// <summary>The payload.</summary>
    [JsonPropertyName("data")]
    public T? Data { get; init; }

    /// <summary>Response metadata; omitted from the wire when null.</summary>
    [JsonPropertyName("meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Meta? Meta { get; init; }
}

/// <summary>
/// Non-generic factory helpers for building <see cref="Envelope{T}"/> values (generic type inference
/// makes these read cleanly at call sites).
/// </summary>
public static class Envelope
{
    /// <summary>Wrap <paramref name="data"/> in an envelope with optional <paramref name="meta"/>.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="data">The payload.</param>
    /// <param name="meta">Optional metadata.</param>
    /// <returns>The envelope.</returns>
    public static Envelope<T> Ok<T>(T data, Meta? meta = null) => new(data, meta);
}
