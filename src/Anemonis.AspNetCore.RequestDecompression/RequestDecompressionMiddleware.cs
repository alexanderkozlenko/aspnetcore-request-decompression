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

#pragma warning disable CA2007

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Represents a middleware for adding HTTP request decompression to the application's request pipeline.</summary>
    public sealed class RequestDecompressionMiddleware : IMiddleware, IDisposable
    {
        private static readonly Dictionary<string, IDecompressionProvider> _defaultProviders = new Dictionary<string, IDecompressionProvider>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, IDecompressionProvider> _providers = new Dictionary<string, IDecompressionProvider>(_defaultProviders, StringComparer.OrdinalIgnoreCase);
        private readonly bool _skipUnsupportedEncodings;
        private readonly ILogger _logger;

        static RequestDecompressionMiddleware()
        {
            foreach (var type in Assembly.GetExecutingAssembly().DefinedTypes)
            {
                if (typeof(IDecompressionProvider).IsAssignableFrom(type) && type.IsNotPublic)
                {
                    var decompressionProvider = (IDecompressionProvider)Activator.CreateInstance(type);
                    var encodingName = type.GetCustomAttribute<EncodingNameAttribute>().EncodingName;

                    _defaultProviders[encodingName] = decompressionProvider;
                }
            }
        }

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

            foreach (var decompressionProviderType in decompressionOptions.Providers)
            {
                var decompressionProvider = (IDecompressionProvider)ActivatorUtilities.CreateInstance(services, decompressionProviderType);
                var encodingName = decompressionProviderType.GetCustomAttribute<EncodingNameAttribute>().EncodingName;

                if (_providers.ContainsKey(encodingName))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Strings.GetString("middleware.duplicate_encoding_name"), encodingName));
                }

                _providers[encodingName] = decompressionProvider;
            }

            _skipUnsupportedEncodings = decompressionOptions.SkipUnsupportedEncodings;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="context" /> is <see langword="null" />.</exception>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.Request.Headers.ContainsKey(HeaderNames.ContentEncoding))
            {
                await next?.Invoke(context);

                return;
            }

            if (context.Request.Headers.ContainsKey(HeaderNames.ContentRange))
            {
                _logger.LogRequestDecodingDisabled();

                await next?.Invoke(context);

                return;
            }

            // There could be a single StringValues entry with comma delimited contents
            //  Content-Encoding: gzip, br, "someEncoding"
            //  string[] { "gzip", "br", "someEncoding"}
            var encodingNamesParsed = context.Request.Headers.GetCommaSeparatedValues(HeaderNames.ContentEncoding);
            if (encodingNamesParsed.Length == 0)
            {
                await next?.Invoke(context);

                return;
            }

            var encodingsLeft = encodingNamesParsed.Length;
            var decodingStream = context.Request.Body;

            for (var i = encodingNamesParsed.Length - 1; i >= 0; i--)
            {
                if (!_providers.TryGetValue(encodingNamesParsed[i], out var provider))
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
                    await decodingStream.CopyToAsync(decodedStream, context.RequestAborted);
                }

                decodedStream.Position = 0L;
                context.Request.Body = decodedStream;
            }

            if (encodingsLeft != encodingNamesParsed.Length)
            {
                if (encodingsLeft == 0)
                {
                    if (context.Request.Body.CanSeek)
                    {
                        context.Request.ContentLength = context.Request.Body.Length;
                    }

                    context.Request.Headers.Remove(HeaderNames.ContentEncoding);
                }
                else if (encodingsLeft == 1)
                {
                    context.Request.Headers[HeaderNames.ContentEncoding] = new StringValues(encodingNamesParsed[0]);
                }
                else
                {
                    var encodingNamesLeft = new string[encodingsLeft];

                    for (var i = 0; i < encodingNamesLeft.Length; i++)
                    {
                        encodingNamesLeft[i] = encodingNamesParsed[i];
                    }

                    context.Request.Headers[HeaderNames.ContentEncoding] = new StringValues(encodingNamesLeft);
                }
            }

            try
            {
                await next?.Invoke(context);
            }
            finally
            {
                decodedStream?.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var provider in _providers.Values)
            {
                (provider as IDisposable)?.Dispose();
            }
        }
    }
}
