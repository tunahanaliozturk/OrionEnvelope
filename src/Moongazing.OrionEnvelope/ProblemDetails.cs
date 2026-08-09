namespace Moongazing.OrionEnvelope;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// An RFC 9457 <c>application/problem+json</c> body: the single failure shape for the whole API. The
/// standard members (<see cref="Type"/>, <see cref="Title"/>, <see cref="Status"/>,
/// <see cref="Detail"/>, <see cref="Instance"/>) sit alongside the family extensions
/// <see cref="TraceId"/> (a correlation handle) and <see cref="Errors"/> (per-field validation
/// messages). Extension members are top-level, per RFC 9457.
/// </summary>
public sealed record ProblemDetails
{
    /// <summary>A URI identifying the problem type; defaults to <c>about:blank</c> (a stable taxonomy of type URIs lands in a later wave).</summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = "about:blank";

    /// <summary>A short, human-readable summary of the problem type.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>The HTTP status code for this occurrence.</summary>
    [JsonPropertyName("status")]
    public int Status { get; init; }

    /// <summary>A human-readable explanation specific to this occurrence.</summary>
    [JsonPropertyName("detail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Detail { get; init; }

    /// <summary>A URI identifying the specific occurrence.</summary>
    [JsonPropertyName("instance")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Instance { get; init; }

    /// <summary>The correlation / trace id, so the same id appears in the response, logs, and traces.</summary>
    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    /// <summary>A machine-readable error code (from the underlying <c>Error.Code</c>).</summary>
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; init; }

    /// <summary>Per-field validation messages, when the problem is a validation failure.</summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ProblemFieldError>? Errors { get; init; }
}

/// <summary>One per-field validation message inside a <see cref="ProblemDetails"/>.</summary>
public sealed record ProblemFieldError
{
    /// <summary>Create a field error.</summary>
    /// <param name="field">The offending field name.</param>
    /// <param name="message">The validation message.</param>
    public ProblemFieldError(string field, string message)
    {
        Field = field;
        Message = message;
    }

    /// <summary>The offending field name.</summary>
    [JsonPropertyName("field")]
    public string Field { get; init; }

    /// <summary>The validation message.</summary>
    [JsonPropertyName("message")]
    public string Message { get; init; }
}
