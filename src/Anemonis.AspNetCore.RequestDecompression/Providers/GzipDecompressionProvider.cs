// © Alexander Kozlenko. Licensed under the MIT License.

using System.IO;
using System.IO.Compression;

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Represents "gzip" decompression provider.</summary>
    [EncodingName("gzip")]
    public sealed class GzipDecompressionProvider : IDecompressionProvider
    {
        /// <summary>Initializes a new instance of the <see cref="GzipDecompressionProvider" /> class.</summary>
        public GzipDecompressionProvider()
        {
        }

        /// <inheritdoc />
        public Stream CreateStream(Stream outputStream)
        {
            return new GZipStream(outputStream, CompressionMode.Decompress);
        }
    }
}
