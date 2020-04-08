# Anemonis.AspNetCore.RequestDecompression

Transparent HTTP request decompression middleware for ASP.NET Core 3 based on the [RFC 7231](https://tools.ietf.org/html/rfc7231#section-3.1.2.2), which serves as a complementary component to the [Microsoft.AspNetCore.ResponseCompression](https://www.nuget.org/packages/Microsoft.AspNetCore.ResponseCompression/) package.

| [![](https://img.shields.io/gitter/room/nwjs/nw.js.svg?style=flat-square)](https://gitter.im/anemonis/aspnetcore-request-decompression) | Release | Current |
|---|---|---|
| Artifacts | [![](https://img.shields.io/nuget/vpre/Anemonis.AspNetCore.RequestDecompression.svg?style=flat-square)](https://www.nuget.org/packages/Anemonis.AspNetCore.RequestDecompression) | [![](https://img.shields.io/myget/alexanderkozlenko/vpre/Anemonis.AspNetCore.RequestDecompression.svg?label=myget&style=flat-square)](https://www.myget.org/feed/alexanderkozlenko/package/nuget/Anemonis.AspNetCore.RequestDecompression) |
| Code Health | | [![](https://img.shields.io/sonar/coverage/aspnetcore-request-decompression?format=long&server=https%3A%2F%2Fsonarcloud.io&style=flat-square)](https://sonarcloud.io/component_measures?id=aspnetcore-request-decompression&metric=coverage&view=list) [![](https://img.shields.io/sonar/violations/aspnetcore-request-decompression?format=long&server=https%3A%2F%2Fsonarcloud.io&style=flat-square)](https://sonarcloud.io/project/issues?id=aspnetcore-request-decompression&resolved=false) |
| Build Status | | [![](https://img.shields.io/azure-devops/build/alexanderkozlenko/github-pipelines/5?label=master&style=flat-square)](https://dev.azure.com/alexanderkozlenko/github-pipelines/_build?definitionId=5&_a=summary) |

## Project Details

- The middleware includes decompression providers for the `gzip`, `DEFLATE`, and `Brotli` algorithms.
- The middleware supports decoding of content with multiple encodings.
- The middleware provides an ability to use a custom provider for the particular encoding.
- The middleware supports automatic response with HTTP status code `415` in case of unsupported encoding.
- The middleware supports the `identity` encoding value.
- The middleware adds the `Content-Length` header if all encodings were handled.

According to the current logging configuration, the following events may appear in a journal:

| ID | Level | Reason |
| :---: | :---: | --- |
| `1100` | `Debug` | The request's content will be decoded with the corresponding provider |
| `1101` | `Debug` | The request's content will not be decoded due to an unknown encoding |
| `1300` | `Warning` | The request's content decoding disabled due to the `Content-Range` header |

## Code Examples

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRequestDecompression(o =>
            {
                o.Providers.Add<DeflateDecompressionProvider>();
                o.Providers.Add<GzipDecompressionProvider>();
                o.Providers.Add<BrotliDecompressionProvider>();
            });

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseRequestDecompression();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```
```cs
[EncodingName("lzma")]
public class LzmaDecompressionProvider : IDecompressionProvider
{
    public Stream CreateStream(Stream outputStream)
    {
        return new LzmaStream(outputStream, CompressionMode.Decompress);
    }
}

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRequestDecompression(o => o.Providers.Add<LzmaDecompressionProvider>());
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseRequestDecompression();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

## Quicklinks

- [Contributing Guidelines](./CONTRIBUTING.md)
- [Code of Conduct](./CODE_OF_CONDUCT.md)
