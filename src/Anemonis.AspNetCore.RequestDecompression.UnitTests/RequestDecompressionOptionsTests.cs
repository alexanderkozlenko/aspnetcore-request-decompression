using System;

using Anemonis.AspNetCore.RequestDecompression.UnitTests.TestStubs;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests
{
    [TestClass]
    public sealed class RequestDecompressionOptionsTests
    {
        [TestMethod]
        public void Constructor()
        {
            var options = new RequestDecompressionOptions();

            Assert.IsTrue(options.SkipUnsupportedEncodings);
            Assert.IsNotNull(options.Providers);
            Assert.AreEqual(0, options.Providers.Count);
        }

        [TestMethod]
        public void AddProvider()
        {
            var options = new RequestDecompressionOptions();

            options.Providers.Add<TestDecompressionProvider10>();

            Assert.IsNotNull(options.Providers);
            Assert.AreEqual(1, options.Providers.Count);
            Assert.IsTrue(options.Providers.Contains(typeof(TestDecompressionProvider10)));
        }

        [TestMethod]
        public void AddProviderWhenCalledTimes()
        {
            var options = new RequestDecompressionOptions();

            options.Providers.Add<TestDecompressionProvider10>();
            options.Providers.Add<TestDecompressionProvider10>();

            Assert.IsNotNull(options.Providers);
            Assert.AreEqual(1, options.Providers.Count);
            Assert.IsTrue(options.Providers.Contains(typeof(TestDecompressionProvider10)));
        }

        [TestMethod]
        public void AddProviderWithTypeWhenTypeIsNull()
        {
            var options = new RequestDecompressionOptions();

            Assert.ThrowsException<ArgumentNullException>(() =>
                options.Providers.Add(null));
        }

        [TestMethod]
        public void AddProviderWithTypeWhenTypeDoesNotHaveAttribute()
        {
            var options = new RequestDecompressionOptions();

            Assert.ThrowsException<ArgumentException>(() =>
                options.Providers.Add(typeof(TestDecompressionProvider00)));
        }

        [TestMethod]
        public void AddProviderWithTypeWhenTypeDoesNotImplementInterface()
        {
            var options = new RequestDecompressionOptions();

            Assert.ThrowsException<ArgumentException>(() =>
                options.Providers.Add(typeof(TestDecompressionProvider01)));
        }

        [TestMethod]
        public void AddProviderWithType()
        {
            var options = new RequestDecompressionOptions();

            options.Providers.Add(typeof(TestDecompressionProvider10));

            Assert.IsNotNull(options.Providers);
            Assert.AreEqual(1, options.Providers.Count);
            Assert.IsTrue(options.Providers.Contains(typeof(TestDecompressionProvider10)));
        }

        [TestMethod]
        public void AddProviderWithTypeWhenCalledTimes()
        {
            var options = new RequestDecompressionOptions();

            options.Providers.Add(typeof(TestDecompressionProvider10));
            options.Providers.Add(typeof(TestDecompressionProvider10));

            Assert.IsNotNull(options.Providers);
            Assert.AreEqual(1, options.Providers.Count);
            Assert.IsTrue(options.Providers.Contains(typeof(TestDecompressionProvider10)));
        }
    }
}
