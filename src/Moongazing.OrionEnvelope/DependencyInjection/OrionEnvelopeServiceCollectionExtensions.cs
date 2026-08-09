namespace Moongazing.OrionEnvelope.DependencyInjection;

using System;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// DI wiring for the envelope.
/// </summary>
public static class OrionEnvelopeServiceCollectionExtensions
{
    /// <summary>
    /// Register the envelope options. The result-shaping filters and exception→problem mapper that
    /// consume these options are added by <c>UseOrionEnvelope</c> in a later wave; registering the
    /// options now keeps the DI convention stable across waves.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration of the envelope options.</param>
    /// <returns>The same <paramref name="services"/>, for chaining.</returns>
    public static IServiceCollection AddOrionEnvelope(this IServiceCollection services, Action<EnvelopeOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<EnvelopeOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }
        return services;
    }
}
