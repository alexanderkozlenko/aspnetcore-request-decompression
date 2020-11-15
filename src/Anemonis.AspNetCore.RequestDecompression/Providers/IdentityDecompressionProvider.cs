// © Alexander Kozlenko. Licensed under the MIT License.

using System.IO;

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Represents "gzip" decompression provider.</summary>
    [EncodingName("identity")]
    public sealed class IdentityDecompressionProvider : IDecompressionProvider
    {
        /// <summary>Initializes a new instance of the <see cref="IdentityDecompressionProvider" /> class.</summary>
        public IdentityDecompressionProvider()
        {
        }

        /// <inheritdoc />
        public Stream CreateStream(Stream outputStream)
        {
            return outputStream;
        }
    }
}
