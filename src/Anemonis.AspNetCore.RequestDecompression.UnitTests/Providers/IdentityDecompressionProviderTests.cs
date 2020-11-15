using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests.Providers
{
    [TestClass]
    public sealed class IdentityDecompressionProviderTests
    {
        [TestMethod]
        public void CreateStream()
        {
            var provider = new IdentityDecompressionProvider() as IDecompressionProvider;
            var stream = provider.CreateStream(new MemoryStream());

            Assert.IsNotNull(stream);
        }
    }
}
