﻿// © Alexander Kozlenko. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Anemonis.AspNetCore.RequestDecompression.Resources;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Represents a middleware for adding HTTP request decompression to the application's request pipeline.</summary>
    public sealed class RequestDecompressionMiddleware : IMiddleware, IDisposable
    {
        private static readonly IDecompressionProvider _identityDecompressionProvider = new IdentityDecompressionProvider();

        private readonly Dictionary<string, IDecompressionProvider> _providers;
        private readonly bool _skipUnsupportedEncodings;
        private readonly ILogger _logger;

        /// <summary>Initializes a new instance of the <see cref="RequestDecompressionMiddleware" /> class.</summary>
        /// <param name="services">The <see cref="IServiceProvider" /> instance for retrieving service objects.</param>
        /// <param name="options">The <see cref="IOptions{T}" /> instance for retrieving decompression options.</param>
        /// <param name="logger">The <see cref="ILogger{T}" /> instance for logging.</param>
        /// <exception cref="ArgumentNullException"><paramref name="services" />, <paramref name="options" />, or <paramref name="logger" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">There are more than one provider registered with the same encoding name.</exception>
        public RequestDecompressionMiddleware(IServiceProvider services, IOptions<RequestDecompressionOptions> options, ILogger<RequestDecompressionMiddleware> logger)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;

            var decompressionOptions = options.Value;

            var decompressionProviders = new Dictionary<string, IDecompressionProvider>(decompressionOptions.Providers.Count, StringComparer.OrdinalIgnoreCase)
            {
                { "identity", _identityDecompressionProvider }
            };

            foreach (var decompressionProviderType in decompressionOptions.Providers)
            {
                var decompressionProvider = (IDecompressionProvider)ActivatorUtilities.CreateInstance(services, decompressionProviderType);
                var encodingName = decompressionProviderType.GetCustomAttribute<EncodingNameAttribute>().EncodingName;

                if (decompressionProviders.ContainsKey(encodingName))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Strings.GetString("middleware.duplicate_encoding_name"), encodingName));
                }

                decompressionProviders[encodingName] = decompressionProvider;
            }

            _providers = decompressionProviders;
            _skipUnsupportedEncodings = decompressionOptions.SkipUnsupportedEncodings;
        }

        /// <summary>Handles an HTTP request as an asynchronous operation.</summary>
        /// <param name="context">The <see cref="HttpContext" /> instance for the current request.</param>
        /// <param name="next">The delegate representing the remaining middleware in the request pipeline.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!context.Request.Headers.TryGetValue(HeaderNames.ContentEncoding, out var encodingNames) && (encodingNames.Count > 0))
            {
                _logger.LogRequestContentIsNotEncoded();

                await next.Invoke(context);

                return;
            }

            _logger.LogRequestContentIsEncoded(encodingNames.Count);

            if (context.Request.Headers.ContainsKey(HeaderNames.ContentRange))
            {
                _logger.LogRequestDecodingDisabled();

                await next.Invoke(context);

                return;
            }

            var encodingsLeft = encodingNames.Count;
            var decodingStream = context.Request.Body;

            for (var i = encodingNames.Count - 1; i >= 0; i--)
            {
                if (!_providers.TryGetValue(encodingNames[i], out var provider))
                {
                    _logger.LogRequestDecodingSkipped();

                    if (!_skipUnsupportedEncodings)
                    {
                        context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;

                        return;
                    }

                    break;
                }

                _logger.LogRequestDecodingApplied(provider.GetType());

                decodingStream = provider.CreateStream(decodingStream);
                encodingsLeft--;
            }

            var decodedStream = default(Stream);

            if (decodingStream != context.Request.Body)
            {
                decodedStream = new MemoryStream();

                using (decodingStream)
                {
                    await decodingStream.CopyToAsync(decodedStream, 81920, context.RequestAborted);
                }

                decodedStream.Position = 0L;
                context.Request.Body = decodedStream;
            }

            if (encodingsLeft == 0)
            {
                context.Request.ContentLength = context.Request.Body.Length;
                context.Request.Headers.Remove(HeaderNames.ContentEncoding);
            }
            else if (encodingsLeft == 1)
            {
                context.Request.Headers[HeaderNames.ContentEncoding] = new StringValues(encodingNames[0]);
            }
            else
            {
                var encodingNamesLeft = new string[encodingsLeft];

                for (var i = 0; i < encodingNamesLeft.Length; i++)
                {
                    encodingNamesLeft[i] = encodingNames[i];
                }

                context.Request.Headers[HeaderNames.ContentEncoding] = new StringValues(encodingNamesLeft);
            }

            try
            {
                await next.Invoke(context);
            }
            finally
            {
                decodedStream?.Dispose();
            }
        }

        /// <summary>Disposes the corresponding decompression providers.</summary>
        public void Dispose()
        {
            foreach (var provider in _providers.Values)
            {
                (provider as IDisposable)?.Dispose();
            }
        }
    }
}
