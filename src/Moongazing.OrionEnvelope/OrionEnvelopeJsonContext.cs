namespace Moongazing.OrionEnvelope;

using System.Text.Json.Serialization;

/// <summary>
/// A <see cref="JsonSerializerContext"/> covering the envelope's non-generic types so they serialize
/// reflection-free (NativeAOT- and trimming-clean). Because <see cref="Envelope{T}"/> is generic, its
/// payload type <c>T</c> is application-specific: add <c>[JsonSerializable(typeof(Envelope&lt;YourDto&gt;))]</c>
/// to your own <see cref="JsonSerializerContext"/> (declaring your payload types alongside) so the
/// whole envelope serializes without reflection.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Meta))]
[JsonSerializable(typeof(PageMeta))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ProblemFieldError))]
public sealed partial class OrionEnvelopeJsonContext : JsonSerializerContext
{
}
