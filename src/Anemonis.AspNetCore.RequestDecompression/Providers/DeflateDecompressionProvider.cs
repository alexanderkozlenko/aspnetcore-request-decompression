// © Alexander Kozlenko. Licensed under the MIT License.

using System.IO;
using System.IO.Compression;

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Represents "DEFLATE" decompression provider.</summary>
    [EncodingName("deflate")]
    public sealed class DeflateDecompressionProvider : IDecompressionProvider
    {
        /// <summary>Initializes a new instance of the <see cref="DeflateDecompressionProvider" /> class.</summary>
        public DeflateDecompressionProvider()
        {
        }

        /// <inheritdoc />
        public Stream CreateStream(Stream outputStream)
        {
            return new DeflateStream(outputStream, CompressionMode.Decompress);
        }
    }
}
