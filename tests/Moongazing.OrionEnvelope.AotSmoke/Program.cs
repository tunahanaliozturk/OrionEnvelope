// NativeAOT smoke test. Publishing this with PublishAot=true must produce zero trim/AOT warnings,
// and running it must exit 0 - that pair is OrionEnvelope's AOT exit criterion. The envelope and
// problem serialize through a source-gen context, so there is no reflection fallback under AOT.
using System.Text.Json;
using System.Text.Json.Serialization;

using Moongazing.OrionEnvelope;

// Serialize/deserialize through the typed JsonTypeInfo from the source-gen context — the AOT-safe
// overloads (the JsonSerializerOptions overloads are RequiresUnreferencedCode).
var envelope = new Envelope<SmokeDto>(
    new SmokeDto(7, "widget"),
    new Meta { TraceId = "trace-1", Page = new PageMeta("cursor-xyz", hasMore: true) });
var envelopeJson = JsonSerializer.Serialize(envelope, SmokeJsonContext.Default.EnvelopeSmokeDto);
var envelopeBack = JsonSerializer.Deserialize(envelopeJson, SmokeJsonContext.Default.EnvelopeSmokeDto);
Check(envelopeBack is { Data.Id: 7 } && envelopeBack.Meta!.Page!.HasMore, "envelope round-trip failed");

// ProblemDetails round-trips.
var problem = new ProblemDetails { Title = "Not Found", Status = 404, Code = "order.missing", TraceId = "trace-1" };
var problemJson = JsonSerializer.Serialize(problem, SmokeJsonContext.Default.ProblemDetails);
var problemBack = JsonSerializer.Deserialize(problemJson, SmokeJsonContext.Default.ProblemDetails);
Check(problemBack is { Status: 404, Code: "order.missing" }, "problem round-trip failed");

Console.WriteLine("OrionEnvelope AOT smoke test passed.");
return 0;

static void Check(bool condition, string message)
{
    if (!condition)
    {
        Console.Error.WriteLine($"AOT smoke test failed: {message}");
        Environment.Exit(1);
    }
}

internal sealed record SmokeDto(int Id, string Name);

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Envelope<SmokeDto>))]
[JsonSerializable(typeof(SmokeDto))]
[JsonSerializable(typeof(Meta))]
[JsonSerializable(typeof(PageMeta))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ProblemFieldError))]
internal sealed partial class SmokeJsonContext : JsonSerializerContext
{
}
