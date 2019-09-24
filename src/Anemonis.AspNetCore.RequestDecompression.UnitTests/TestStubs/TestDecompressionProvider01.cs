using System;
using System.IO;

#pragma warning disable IDE0060

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests.TestStubs
{
    [EncodingName("encoding")]
    internal sealed class TestDecompressionProvider01
    {
        public Stream CreateStream(Stream outputStream)
        {
            throw new NotImplementedException();
        }
    }
}
