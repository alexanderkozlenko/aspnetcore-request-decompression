// © Alexander Kozlenko. Licensed under the MIT License.

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
        private readonly Dictionary<string, IDecompressionProvider> _providers;
        private readonly bool _skipUnsupportedEncodings;
        private readonly ILogger _logger;

        /// <summary>Initializes a new instance of the <see cref="RequestDecompressionMiddleware" /> class.</summary>
        /// <param name="services">The <see cref="IServiceProvider" /> instance for retrieving service objects.</param>
        /// <param name="options">The <see cref="IOptions{T}" /> instance for retrieving decompression options.</param>
        /// <param name="logger">The <see cref="ILogger{T}" /> instance for logging.</param>
        /// <exception cref="InvalidOperationException">A decompression provider registered with encoding name specified.</exception>
        public RequestDecompressionMiddleware(IServiceProvider services, IOptions<RequestDecompressionOptions> options, ILogger<RequestDecompressionMiddleware> logger)
        {
            _providers = CreateProviders(services, options.Value.Providers);
            _skipUnsupportedEncodings = options.Value.SkipUnsupportedEncodings;
            _logger = logger;
        }

        /// <inheritdoc />
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var request = context.Request;
            var requestHeaders = request.Headers;

            // There could be a single StringValues entry with comma delimited contents

            var encodingNames = requestHeaders.GetCommaSeparatedValues(HeaderNames.ContentEncoding);

            if (encodingNames.Length == 0)
            {
                return next.Invoke(context);
            }

            if (requestHeaders.ContainsKey(HeaderNames.ContentRange))
            {
                _logger.LogRequestDecodingDisabled();

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
            foreach (var kvp in _providers)
            {
                (kvp.Value as IDisposable)?.Dispose();
            }
        }

        private static Dictionary<string, IDecompressionProvider> CreateProviders(IServiceProvider services, IReadOnlyCollection<Type> providerTypes)
        {
            var providers = new Dictionary<string, IDecompressionProvider>(providerTypes.Count, StringComparer.OrdinalIgnoreCase);

            foreach (var providerType in providerTypes)
            {
                var encodingNameAttribute = providerType.GetCustomAttribute<EncodingNameAttribute>();

                if (encodingNameAttribute is null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.GetString("middleware.no_encoding_name"), providerType));
                }

                providers[encodingNameAttribute.EncodingName] = (IDecompressionProvider)ActivatorUtilities.CreateInstance(services, providerType);
            }

            return providers;
        }
    }
}
