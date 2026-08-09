namespace Moongazing.OrionEnvelope;

using System;
using System.Collections.Generic;

using Moongazing.OrionResult;

/// <summary>
/// The mechanical projection between the family's internal <see cref="Result{T}"/> vocabulary and the
/// HTTP wire shapes: a success becomes an <see cref="Envelope{T}"/>, a failure becomes an RFC 9457
/// <see cref="ProblemDetails"/> with a status derived from the error's <see cref="ErrorKind"/>. This
/// is the whole point — an endpoint that returns a <c>Result</c> is correctly shaped with no
/// hand-written envelope code. The ASP.NET filters that call these automatically arrive in a later wave.
/// </summary>
public static class EnvelopeMapper
{
    /// <summary>Project a successful result into a success envelope.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="result">A successful result.</param>
    /// <param name="meta">Optional metadata to attach.</param>
    /// <returns>The success envelope.</returns>
    /// <exception cref="InvalidOperationException">The result is a failure — map it with <see cref="ToProblemDetails{T}"/>.</exception>
    public static Envelope<T> ToEnvelope<T>(this Result<T> result, Meta? meta = null)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException("Cannot wrap a failed Result in a success envelope; map it with ToProblemDetails instead.");
        }
        return new Envelope<T>(result.Value, meta);
    }

    /// <summary>Project a failed result into an RFC 9457 problem, aggregating any validation field errors.</summary>
    /// <typeparam name="T">The result's payload type.</typeparam>
    /// <param name="result">A failed result.</param>
    /// <param name="traceId">Optional correlation id to surface as <c>traceId</c>.</param>
    /// <param name="instance">Optional URI identifying this occurrence.</param>
    /// <returns>The problem details.</returns>
    /// <exception cref="InvalidOperationException">The result is a success.</exception>
    public static ProblemDetails ToProblemDetails<T>(this Result<T> result, string? traceId = null, string? instance = null)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot map a successful Result to ProblemDetails.");
        }

        var primary = result.Errors[0];
        var fields = AggregateFields(result.Errors);
        return new ProblemDetails
        {
            Type = "about:blank",
            Title = TitleFor(primary.Kind),
            Status = StatusFor(primary.Kind),
            Detail = primary.Message,
            Code = primary.Code,
            TraceId = traceId,
            Instance = instance,
            Errors = fields,
        };
    }

    /// <summary>Project a single error into an RFC 9457 problem.</summary>
    /// <param name="error">The error.</param>
    /// <param name="traceId">Optional correlation id.</param>
    /// <param name="instance">Optional occurrence URI.</param>
    /// <returns>The problem details.</returns>
    public static ProblemDetails ToProblemDetails(this Error error, string? traceId = null, string? instance = null) =>
        new()
        {
            Type = "about:blank",
            Title = TitleFor(error.Kind),
            Status = StatusFor(error.Kind),
            Detail = error.Message,
            Code = error.Code,
            TraceId = traceId,
            Instance = instance,
            Errors = MapFields(error.Fields),
        };

    /// <summary>The HTTP status code the family maps an <see cref="ErrorKind"/> to.</summary>
    /// <param name="kind">The error kind.</param>
    /// <returns>The HTTP status code.</returns>
    public static int StatusFor(ErrorKind kind) => kind switch
    {
        ErrorKind.Validation => 400,
        ErrorKind.NotFound => 404,
        ErrorKind.Conflict => 409,
        ErrorKind.FailedPrecondition => 422,
        ErrorKind.Unauthorized => 401,
        ErrorKind.Forbidden => 403,
        ErrorKind.RateLimited => 429,
        ErrorKind.Timeout => 504,
        ErrorKind.Unavailable => 503,
        ErrorKind.Unexpected => 500,
        _ => 500,
    };

    private static string TitleFor(ErrorKind kind) => kind switch
    {
        ErrorKind.Validation => "Validation Failed",
        ErrorKind.NotFound => "Not Found",
        ErrorKind.Conflict => "Conflict",
        ErrorKind.FailedPrecondition => "Failed Precondition",
        ErrorKind.Unauthorized => "Unauthorized",
        ErrorKind.Forbidden => "Forbidden",
        ErrorKind.RateLimited => "Too Many Requests",
        ErrorKind.Timeout => "Timeout",
        ErrorKind.Unavailable => "Service Unavailable",
        ErrorKind.Unexpected => "Internal Server Error",
        _ => "Internal Server Error",
    };

    private static ProblemFieldError[]? MapFields(IReadOnlyList<FieldError>? fields)
    {
        if (fields is null || fields.Count == 0)
        {
            return null;
        }
        var mapped = new ProblemFieldError[fields.Count];
        for (var i = 0; i < fields.Count; i++)
        {
            mapped[i] = new ProblemFieldError(fields[i].Field, fields[i].Message);
        }
        return mapped;
    }

    private static List<ProblemFieldError>? AggregateFields(IReadOnlyList<Error> errors)
    {
        List<ProblemFieldError>? all = null;
        foreach (var error in errors)
        {
            if (error.Fields is null)
            {
                continue;
            }
            all ??= new List<ProblemFieldError>();
            foreach (var field in error.Fields)
            {
                all.Add(new ProblemFieldError(field.Field, field.Message));
            }
        }
        return all;
    }
}
