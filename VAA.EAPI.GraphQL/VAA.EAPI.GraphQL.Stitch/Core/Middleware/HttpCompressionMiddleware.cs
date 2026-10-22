using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.IO.Compression;
using VAA.EAPI.GraphQL.Stitch.Core.Extensions;

namespace VAA.EAPI.GraphQL.Stitch.Core.Middleware
{
    public sealed class HttpCompressionMiddleware : IFunctionsWorkerMiddleware
    {
        public const string AcceptEncodingHeader = "Accept-Encoding";
        public const string ContentEncodingHeader = "Content-Encoding";

        public static class AcceptCompressionNames
        {
            public const string Brotli = "br";
            public const string Deflate = "deflate";
            public const string Gzip = "gzip";

            public static readonly string[] Headers = new[] { Brotli, Deflate, Gzip };
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            if (!context.FunctionDefinition.InputBindings.Any(x => x.Value.Type.Is("httpTrigger")))
            {
                await next(context);

                return;
            }

            /// now we know the real truth, http compression is only supported in azure 
            /// functions if hosted in IIS utilising dynamic compression, since ours are
            /// isloated functions we have to take care of this ourselves

            var httpRequestData = await context.GetHttpRequestDataAsync();
            var compresssionRequest = httpRequestData.Headers.TryGetValues(AcceptEncodingHeader, out IEnumerable<string> acceptCompressionTypes) && acceptCompressionTypes.Any(v => AcceptCompressionNames.Headers.Any(c => c.Is(v)));

            await next(context);

            if (compresssionRequest)
            {
                var httpResponseData = context.GetHttpResponseData();

                if (httpResponseData is null) return;

                var body = (MemoryStream)httpResponseData.Body;
                var memoryStream = new MemoryStream();

                body.Position = 0;

                if (acceptCompressionTypes.Any(x => x.Is(AcceptCompressionNames.Gzip)))
                {
                    httpResponseData.Headers.AddOrUpdate(ContentEncodingHeader, AcceptCompressionNames.Gzip);

                    using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true);

                    body.CopyTo(gZipStream);

                    gZipStream.Flush();
                }

                else if (acceptCompressionTypes.Any(x => x.Is(AcceptCompressionNames.Deflate)))
                {
                    httpResponseData.Headers.AddOrUpdate(ContentEncodingHeader, AcceptCompressionNames.Deflate);

                    using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true);

                    body.CopyTo(deflateStream);

                    deflateStream.Flush();
                }
                else if (acceptCompressionTypes.Any(x => x.Is(AcceptCompressionNames.Brotli)))
                {
                    httpResponseData.Headers.AddOrUpdate(ContentEncodingHeader, AcceptCompressionNames.Brotli);

                    using var brotliStream = new BrotliStream(memoryStream, CompressionMode.Compress, true);

                    body.CopyTo(brotliStream);

                    brotliStream.Flush();
                }

                httpResponseData.Body = memoryStream;

                context.SetHttpResponseData(httpResponseData);
            }
        }
    }
}
