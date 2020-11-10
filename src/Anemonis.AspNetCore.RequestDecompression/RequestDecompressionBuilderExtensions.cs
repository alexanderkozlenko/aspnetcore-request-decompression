// © Alexander Kozlenko. Licensed under the MIT License.

using System;

using Anemonis.AspNetCore.RequestDecompression;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>The HTTP request decompression middleware extensions for the <see cref="IApplicationBuilder" />.</summary>
    public static class RequestDecompressionBuilderExtensions
    {
        /// <summary>Adds the HTTP request decompression middleware to the application's request pipeline.</summary>
        /// <param name="builder">The <see cref="IApplicationBuilder" /> to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="builder" /> is <see langword="null" />.</exception>
        public static IApplicationBuilder UseRequestDecompression(this IApplicationBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<RequestDecompressionMiddleware>();
        }
    }
}
