// © Alexander Kozlenko. Licensed under the MIT License.

using System.IO;

#pragma warning disable CA1812

namespace Anemonis.AspNetCore.RequestDecompression
{
    [EncodingName("identity")]
    internal sealed class IdentityDecompressionProvider : IDecompressionProvider
    {
        public Stream CreateStream(Stream outputStream)
        {
            return outputStream;
        }
    }
}
