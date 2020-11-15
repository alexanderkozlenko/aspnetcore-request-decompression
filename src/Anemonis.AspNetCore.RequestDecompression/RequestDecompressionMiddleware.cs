﻿// © Alexander Kozlenko. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

using Anemonis.AspNetCore.RequestDecompression.Resources;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Represents a middleware for adding HTTP request decompression to the application's request pipeline.</summary>
    public sealed class RequestDecompressionMiddleware : IMiddleware, IDisposable
    {
        private readonly Dictionary<string, IDecompressionProvider> _providers = new(StringComparer.OrdinalIgnoreCase);
        private readonly bool _skipUnsupportedEncodings;
        private readonly ILogger _logger;

        /// <summary>Initializes a new instance of the <see cref="RequestDecompressionMiddleware" /> class.</summary>
        /// <param name="services">The <see cref="IServiceProvider" /> instance for retrieving service objects.</param>
        /// <param name="options">The <see cref="IOptions{T}" /> instance for retrieving decompression options.</param>
        /// <param name="logger">The <see cref="ILogger{T}" /> instance for logging.</param>
        /// <exception cref="InvalidOperationException">A decompression provider registered with encoding name specified.</exception>
        public RequestDecompressionMiddleware(IServiceProvider services, IOptions<RequestDecompressionOptions> options, ILogger<RequestDecompressionMiddleware> logger)
        {
            _logger = logger;

            var decompressionOptions = options.Value;

            foreach (var decompressionProviderType in decompressionOptions.Providers)
            {
                var decompressionProvider = (IDecompressionProvider)ActivatorUtilities.CreateInstance(services, decompressionProviderType);
                var encodingNameAttribute = decompressionProviderType.GetCustomAttribute<EncodingNameAttribute>();

                if (encodingNameAttribute is null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Strings.GetString("middleware.no_encoding_name"), decompressionProvider.GetType()));
                }

                var encodingName = encodingNameAttribute.EncodingName;

                _providers[encodingName] = decompressionProvider;
            }

            _skipUnsupportedEncodings = decompressionOptions.SkipUnsupportedEncodings;
        }

        /// <inheritdoc />
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var request = context.Request;
            var requestHeaders = request.Headers;

            if (!requestHeaders.ContainsKey(HeaderNames.ContentEncoding))
            {
                return next.Invoke(context);
            }

            if (requestHeaders.ContainsKey(HeaderNames.ContentRange))
            {
                _logger.LogRequestDecodingDisabled();

                return next.Invoke(context);
            }

            // There could be a single StringValues entry with comma delimited contents

            var encodingNames = requestHeaders.GetCommaSeparatedValues(HeaderNames.ContentEncoding);

            if (encodingNames.Length == 0)
            {
                return next.Invoke(context);
            }

            var encodingsLeft = encodingNames.Length;
            var requestBody = request.Body;

            for (var i = encodingNames.Length - 1; i >= 0; i--)
            {
                if (!_providers.TryGetValue(encodingNames[i], out var provider))
                {
                    _logger.LogRequestDecodingSkipped();

                    if (!_skipUnsupportedEncodings)
                    {
                        context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;

                        return Task.CompletedTask;
                    }

                    break;
                }

                requestBody = provider.CreateStream(requestBody);
                encodingsLeft--;

                _logger.LogRequestDecodingApplied(provider);
            }

            request.Body = requestBody;

            if (encodingsLeft != encodingNames.Length)
            {
                if (encodingsLeft == 0)
                {
                    if (requestBody.CanSeek)
                    {
                        request.ContentLength = requestBody.Length;
                    }

                    requestHeaders.Remove(HeaderNames.ContentEncoding);
                }
                else if (encodingsLeft == 1)
                {
                    requestHeaders[HeaderNames.ContentEncoding] = new(encodingNames[0]);
                }
                else
                {
                    var encodingNamesLeft = new string[encodingsLeft];

                    for (var i = 0; i < encodingNamesLeft.Length; i++)
                    {
                        encodingNamesLeft[i] = encodingNames[i];
                    }

                    requestHeaders[HeaderNames.ContentEncoding] = new(encodingNamesLeft);
                }
            }

            return next.Invoke(context);
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
