namespace Moongazing.OrionEnvelope.Tests;

using System;
using System.Linq;

using Moongazing.OrionResult;

using Xunit;

/// <summary>The projection from OrionResult's Result/Error to the wire envelope and problem shapes.</summary>
public sealed class EnvelopeMappingTests
{
    [Fact]
    public void A_successful_result_projects_to_a_success_envelope()
    {
        Result<int> result = 42;
        var meta = new Meta { TraceId = "abc" };

        var envelope = result.ToEnvelope(meta);

        Assert.Equal(42, envelope.Data);
        Assert.Equal("abc", envelope.Meta!.TraceId);
    }

    [Fact]
    public void Enveloping_a_failed_result_throws()
    {
        Result<int> result = Error.NotFound("order.missing", "No such order.");
        Assert.Throws<InvalidOperationException>(() => result.ToEnvelope());
    }

    [Fact]
    public void A_failed_result_projects_to_problem_details_with_the_mapped_status()
    {
        Result<int> result = Error.NotFound("order.missing", "No such order.");

        var problem = result.ToProblemDetails(traceId: "trace-1");

        Assert.Equal(404, problem.Status);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal("No such order.", problem.Detail);
        Assert.Equal("order.missing", problem.Code);
        Assert.Equal("trace-1", problem.TraceId);
        Assert.Equal("about:blank", problem.Type);
        Assert.Null(problem.Errors);
    }

    [Fact]
    public void A_validation_result_aggregates_field_errors()
    {
        var error = Error.Validation("bad.input", "Validation failed.", new[]
        {
            new FieldError("email", "required"),
            new FieldError("age", "must be positive"),
        });
        Result<int> result = error;

        var problem = result.ToProblemDetails();

        Assert.Equal(400, problem.Status);
        Assert.NotNull(problem.Errors);
        Assert.Equal(2, problem.Errors!.Count);
        Assert.Contains(problem.Errors, e => e.Field == "email" && e.Message == "required");
    }

    [Theory]
    [InlineData(ErrorKind.Validation, 400)]
    [InlineData(ErrorKind.NotFound, 404)]
    [InlineData(ErrorKind.Conflict, 409)]
    [InlineData(ErrorKind.FailedPrecondition, 422)]
    [InlineData(ErrorKind.Unauthorized, 401)]
    [InlineData(ErrorKind.Forbidden, 403)]
    [InlineData(ErrorKind.RateLimited, 429)]
    [InlineData(ErrorKind.Timeout, 504)]
    [InlineData(ErrorKind.Unavailable, 503)]
    [InlineData(ErrorKind.Unexpected, 500)]
    public void Every_error_kind_maps_to_its_http_status(ErrorKind kind, int expected)
    {
        Assert.Equal(expected, EnvelopeMapper.StatusFor(kind));
    }

    [Fact]
    public void Mapping_a_successful_result_to_problem_details_throws()
    {
        Result<int> result = 1;
        Assert.Throws<InvalidOperationException>(() => result.ToProblemDetails());
    }

    [Fact]
    public void Meta_is_empty_when_all_members_are_null()
    {
        Assert.True(new Meta().IsEmpty);
        Assert.False(new Meta { TraceId = "x" }.IsEmpty);
        Assert.False(new Meta { Page = new PageMeta("c", true) }.IsEmpty);
    }
}
