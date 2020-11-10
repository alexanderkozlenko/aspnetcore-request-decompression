// © Alexander Kozlenko. Licensed under the MIT License.

using System;

using Anemonis.AspNetCore.RequestDecompression;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>The HTTP request decompression middleware extensions for the <see cref="IServiceCollection" />.</summary>
    public static class RequestDecompressionServicesExtensions
    {
        /// <summary>Adds the HTTP request decompression middleware services to the current <see cref="IServiceCollection" /> instance.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> instance to add the services to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="services" /> is <see langword="null" />.</exception>
        public static IServiceCollection AddRequestDecompression(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<RequestDecompressionMiddleware, RequestDecompressionMiddleware>();

            return services;
        }

        /// <summary>Adds the HTTP request decompression middleware services to the current <see cref="IServiceCollection" /> instance.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> instance to add the services to.</param>
        /// <param name="configureOptions">The delegate to configure a <see cref="RequestDecompressionOptions" />.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="services" /> or <paramref name="configureOptions" /> is <see langword="null" />.</exception>
        public static IServiceCollection AddRequestDecompression(this IServiceCollection services, Action<RequestDecompressionOptions> configureOptions)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (configureOptions is null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.Configure(configureOptions);
            services.TryAddSingleton<RequestDecompressionMiddleware, RequestDecompressionMiddleware>();

            return services;
        }
    }
}
