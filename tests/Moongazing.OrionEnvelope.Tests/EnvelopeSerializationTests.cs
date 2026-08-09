namespace Moongazing.OrionEnvelope.Tests;

using System.Text.Json;
using System.Text.Json.Serialization;

using Xunit;

/// <summary>
/// Serialization tests proving the envelope round-trips under a source-gen context with NO reflection
/// fallback (the AOT exit criterion), and golden-file tests pinning the exact wire shape.
/// </summary>
public sealed class EnvelopeSerializationTests
{
    // The context's own options: its TypeInfoResolver is the source-gen context and nothing else, so
    // if any type were not covered by source generation, serialization would throw instead of
    // silently falling back to reflection. Passing therefore proves full source-gen coverage, and the
    // baked [JsonSourceGenerationOptions] (camelCase + omit-null) apply.
    private static readonly JsonSerializerOptions SourceGenOnly = TestJsonContext.Default.Options;

    [Fact]
    public void Envelope_round_trips_under_source_gen_with_no_reflection_fallback()
    {
        var envelope = new Envelope<TestDto>(
            new TestDto(7, "widget"),
            new Meta { TraceId = "trace-1", Page = new PageMeta("cursor-xyz", hasMore: true) });

        var json = JsonSerializer.Serialize(envelope, SourceGenOnly);
        var back = JsonSerializer.Deserialize<Envelope<TestDto>>(json, SourceGenOnly);

        Assert.NotNull(back);
        Assert.Equal(7, back!.Data!.Id);
        Assert.Equal("widget", back.Data.Name);
        Assert.Equal("trace-1", back.Meta!.TraceId);
        Assert.Equal("cursor-xyz", back.Meta.Page!.NextCursor);
        Assert.True(back.Meta.Page.HasMore);
    }

    [Fact]
    public void ProblemDetails_round_trips_under_source_gen()
    {
        var problem = new ProblemDetails
        {
            Title = "Not Found",
            Status = 404,
            Detail = "No such order.",
            Code = "order.missing",
            TraceId = "trace-1",
        };

        var json = JsonSerializer.Serialize(problem, SourceGenOnly);
        var back = JsonSerializer.Deserialize<ProblemDetails>(json, SourceGenOnly);

        Assert.Equal(404, back!.Status);
        Assert.Equal("order.missing", back.Code);
    }

    [Fact]
    public void Success_envelope_json_shape_is_pinned()
    {
        var envelope = new Envelope<TestDto>(new TestDto(1, "a"), new Meta { TraceId = "t" });
        var json = JsonSerializer.Serialize(envelope, SourceGenOnly);
        Assert.Equal("{\"data\":{\"id\":1,\"name\":\"a\"},\"meta\":{\"traceId\":\"t\"}}", json);
    }

    [Fact]
    public void Paged_meta_json_shape_is_pinned()
    {
        var meta = new Meta { TraceId = "t", Page = new PageMeta("eyJ", hasMore: true) };
        var json = JsonSerializer.Serialize(meta, SourceGenOnly);
        Assert.Equal("{\"traceId\":\"t\",\"page\":{\"nextCursor\":\"eyJ\",\"hasMore\":true}}", json);
    }

    [Fact]
    public void Null_meta_is_omitted_from_a_success_envelope()
    {
        var envelope = new Envelope<TestDto>(new TestDto(1, "a"));
        var json = JsonSerializer.Serialize(envelope, SourceGenOnly);
        Assert.Equal("{\"data\":{\"id\":1,\"name\":\"a\"}}", json);
    }

    [Fact]
    public void Problem_details_omits_null_members_and_defaults_type()
    {
        var problem = new ProblemDetails { Title = "Conflict", Status = 409 };
        var json = JsonSerializer.Serialize(problem, SourceGenOnly);
        Assert.Equal("{\"type\":\"about:blank\",\"title\":\"Conflict\",\"status\":409}", json);
    }
}

public sealed record TestDto(int Id, string Name);

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Envelope<TestDto>))]
[JsonSerializable(typeof(TestDto))]
[JsonSerializable(typeof(Meta))]
[JsonSerializable(typeof(PageMeta))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ProblemFieldError))]
internal sealed partial class TestJsonContext : JsonSerializerContext
{
}
