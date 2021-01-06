// © Alexander Kozlenko. Licensed under the MIT License.

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Provides options for the HTTP request decompression middleware.</summary>
    public sealed class RequestDecompressionOptions
    {
        /// <summary>Initializes a new instance of the <see cref="RequestDecompressionOptions" /> class.</summary>
        public RequestDecompressionOptions()
        {
            Providers = new();
            SkipUnsupportedEncodings = true;
        }

        /// <summary>Gets a collection of registered decompression provider types.</summary>
        public RequestDecompressionProviderCollection Providers
        {
            get;
        }

        /// <summary>Gets or sets the value indicating whether the middleware should pass content encoded with unsupported encoding to the next middleware in the request pipeline.</summary>
        public bool SkipUnsupportedEncodings
        {
            get;
            set;
        }
    }
}
