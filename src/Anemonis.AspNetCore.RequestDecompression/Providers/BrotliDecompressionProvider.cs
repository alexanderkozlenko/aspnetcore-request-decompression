// © Alexander Kozlenko. Licensed under the MIT License.

using System.IO;
using System.IO.Compression;

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Represents "Brotli" decompression provider.</summary>
    [EncodingName("br")]
    public sealed class BrotliDecompressionProvider : IDecompressionProvider
    {
        /// <summary>Initializes a new instance of the <see cref="BrotliDecompressionProvider" /> class.</summary>
        public BrotliDecompressionProvider()
        {
        }

        Stream IDecompressionProvider.CreateStream(Stream outputStream)
        {
            return new BrotliStream(outputStream, CompressionMode.Decompress);
        }
    }
}
