using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests.Providers
{
    [TestClass]
    public sealed class DeflateDecompressionProviderTests
    {
        [TestMethod]
        public void CreateStream()
        {
            var provider = new DeflateDecompressionProvider();
            var stream = provider.CreateStream(new MemoryStream());

            Assert.IsNotNull(stream);
        }
    }
}
