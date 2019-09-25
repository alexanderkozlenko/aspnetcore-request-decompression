using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Anemonis.AspNetCore.RequestDecompression.UnitTests.TestStubs;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests
{
    [TestClass]
    public sealed partial class RequestDecompressionMiddlewareTests
    {
        [TestMethod]
        public void ConstructorWithServicesAndOptionsWhenServicesIsNull()
        {
            var optionsMock = new Mock<IOptions<RequestDecompressionOptions>>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger<RequestDecompressionMiddleware>>(MockBehavior.Loose);

            Assert.ThrowsException<ArgumentNullException>(() =>
                new RequestDecompressionMiddleware(null, optionsMock.Object, loggerMock.Object));
        }

        [TestMethod]
        public void ConstructorWithServicesAndOptionsWhenOptionsIsNull()
        {
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger<RequestDecompressionMiddleware>>(MockBehavior.Loose);

            Assert.ThrowsException<ArgumentNullException>(() =>
                new RequestDecompressionMiddleware(serviceProviderMock.Object, null, loggerMock.Object));
        }

        [TestMethod]
        public void ConstructorWithServicesAndOptionsWhenLoggerIsNull()
        {
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var optionsMock = new Mock<IOptions<RequestDecompressionOptions>>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(() =>
                new RequestDecompressionMiddleware(serviceProviderMock.Object, optionsMock.Object, null));
        }

        [TestMethod]
        public void ConstructorWithDuplicateEncodings()
        {
            var options = new RequestDecompressionOptions();

            options.Providers.Add<TestDecompressionProvider20>();
            options.Providers.Add<TestDecompressionProvider21>();

            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var optionsMock = new Mock<IOptions<RequestDecompressionOptions>>(MockBehavior.Strict);

            optionsMock
                .Setup(o => o.Value)
                .Returns(options);

            var loggerMock = new Mock<ILogger<RequestDecompressionMiddleware>>(MockBehavior.Loose);

            Assert.ThrowsException<InvalidOperationException>(() =>
                new RequestDecompressionMiddleware(serviceProviderMock.Object, optionsMock.Object, loggerMock.Object));
        }

        [TestMethod]
        public async Task InvokeAsyncWhenRequestHasContentRangeHeader()
        {
            var options = new RequestDecompressionOptions();

            options.Providers.Add<TestDecompressionProvider10>();

            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var optionsMock = new Mock<IOptions<RequestDecompressionOptions>>(MockBehavior.Strict);

            optionsMock
                .Setup(o => o.Value)
                .Returns(options);

            var loggerMock = new Mock<ILogger<RequestDecompressionMiddleware>>(MockBehavior.Loose);

            loggerMock
                .Setup(o => o.IsEnabled(It.IsAny<LogLevel>()))
                .Returns(true);

            var middleware = new RequestDecompressionMiddleware(serviceProviderMock.Object, optionsMock.Object, loggerMock.Object);
            var content = "Hello World!";
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Method = HttpMethods.Post;
            httpContext.Request.Headers.Add(HeaderNames.ContentEncoding, "encoding");
            httpContext.Request.Headers.Add(HeaderNames.ContentRange, "0-*/*");
            httpContext.Request.Body = new TestRequestStream(contentBytes);

            await middleware.InvokeAsync(httpContext, c => Task.CompletedTask);

            Assert.AreEqual(0, httpContext.Response.Body.Length);
        }

        [DataTestMethod]
        [DataRow("", "", true, StatusCodes.Status200OK)]
        [DataRow("identity", "", true, StatusCodes.Status200OK)]
        [DataRow("deflate", "", true, StatusCodes.Status200OK)]
        [DataRow("gzip", "", true, StatusCodes.Status200OK)]
        [DataRow("br", "", true, StatusCodes.Status200OK)]
        [DataRow("unknown", "unknown", true, StatusCodes.Status200OK)]
        [DataRow("unknown", "", false, StatusCodes.Status415UnsupportedMediaType)]
        [DataRow("identity deflate gzip br", "", true, StatusCodes.Status200OK)]
        [DataRow("identity deflate gzip br", "", false, StatusCodes.Status200OK)]
        [DataRow("unknown deflate gzip br", "unknown", true, StatusCodes.Status200OK)]
        [DataRow("unknown deflate gzip br", "", false, StatusCodes.Status415UnsupportedMediaType)]
        [DataRow("identity unknown deflate gzip br", "identity unknown", true, StatusCodes.Status200OK)]
        [DataRow("identity unknown deflate gzip br", "", false, StatusCodes.Status415UnsupportedMediaType)]
        public async Task InvokeAsync(string encoding1, string encoding2, bool skipUnsupportedEncodings, int statusCode)
        {
            var options = new RequestDecompressionOptions();

            options.Providers.Add<DeflateDecompressionProvider>();
            options.Providers.Add<GzipDecompressionProvider>();
            options.Providers.Add<BrotliDecompressionProvider>();
            options.SkipUnsupportedEncodings = skipUnsupportedEncodings;

            var loggerMock = new Mock<ILogger<RequestDecompressionMiddleware>>(MockBehavior.Loose);

            loggerMock
                .Setup(o => o.IsEnabled(It.IsAny<LogLevel>()))
                .Returns(true);

            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var optionsMock = new Mock<IOptions<RequestDecompressionOptions>>(MockBehavior.Strict);

            optionsMock
                .Setup(o => o.Value)
                .Returns(options);

            var middleware = new RequestDecompressionMiddleware(serviceProviderMock.Object, optionsMock.Object, loggerMock.Object);
            var content = "Hello World!";

            var contentBytes1 = Encoding.UTF8.GetBytes(content);
            var contentBytes2 = default(byte[]);

            var encoding1Values = new StringValues(encoding1.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var encoding2Values = new StringValues(encoding2.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            foreach (var encoding in encoding1Values)
            {
                contentBytes1 = CompressionEncoder.Encode(contentBytes1, encoding);
            }

            var httpContext = new DefaultHttpContext();

            httpContext.Request.Method = HttpMethods.Post;
            httpContext.Request.Headers.Add(HeaderNames.ContentEncoding, encoding1Values);
            httpContext.Request.Body = new TestRequestStream(contentBytes1);

            await middleware.InvokeAsync(httpContext, c => Task.CompletedTask);

            if (statusCode == StatusCodes.Status200OK)
            {
                Assert.AreEqual(encoding2Values, httpContext.Request.Headers[HeaderNames.ContentEncoding]);

                if (encoding2 == "")
                {
                    if (httpContext.Request.Body is TestRequestStream)
                    {
                        contentBytes2 = ((TestRequestStream)httpContext.Request.Body).ToArray();
                    }
                    else
                    {
                        contentBytes2 = ((MemoryStream)httpContext.Request.Body).ToArray();
                    }


                    Assert.AreEqual(content, Encoding.UTF8.GetString(contentBytes2));
                }
            }

            Assert.AreEqual(statusCode, httpContext.Response.StatusCode);
        }
    }
}
