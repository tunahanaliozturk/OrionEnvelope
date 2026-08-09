namespace Moongazing.OrionEnvelope;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Response metadata that rides alongside <see cref="Envelope{T}.Data"/>: a correlation
/// <see cref="TraceId"/> a client can quote in a support ticket, optional
/// <see cref="Page"/> pagination info, and an optional <see cref="Warnings"/> channel (e.g.
/// deprecation notices) that never disturbs the <c>data</c> contract. All members are omitted from the
/// wire when null, so a response with no metadata carries an empty <c>meta</c> — or none at all.
/// </summary>
public sealed record Meta
{
    /// <summary>The correlation / trace id for this response.</summary>
    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    /// <summary>Pagination metadata when the payload is a page.</summary>
    [JsonPropertyName("page")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PageMeta? Page { get; init; }

    /// <summary>Non-fatal warnings (e.g. deprecation notices) that do not change the payload.</summary>
    [JsonPropertyName("warnings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Warnings { get; init; }

    /// <summary>Whether every member is null (so the whole <c>meta</c> can be omitted).</summary>
    [JsonIgnore]
    public bool IsEmpty => TraceId is null && Page is null && Warnings is null;
}

/// <summary>
/// Pagination metadata folded into <see cref="Meta.Page"/>: the opaque forward cursor for the next
/// page and whether more rows exist. Mirrors a keyset page's cursor contract; the automatic
/// projection from <c>OrionPage</c>'s <c>Page&lt;T&gt;</c> ships in a later wave, but the shape lives
/// here so the envelope contract is complete now.
/// </summary>
public sealed record PageMeta
{
    /// <summary>Create pagination metadata.</summary>
    /// <param name="nextCursor">The opaque cursor for the next page, or null when there is none.</param>
    /// <param name="hasMore">Whether at least one more row exists after this page.</param>
    public PageMeta(string? nextCursor, bool hasMore)
    {
        NextCursor = nextCursor;
        HasMore = hasMore;
    }

    /// <summary>The opaque cursor for the next page, or null when there is no next page.</summary>
    [JsonPropertyName("nextCursor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NextCursor { get; init; }

    /// <summary>Whether at least one more row exists after this page.</summary>
    [JsonPropertyName("hasMore")]
    public bool HasMore { get; init; }
}
