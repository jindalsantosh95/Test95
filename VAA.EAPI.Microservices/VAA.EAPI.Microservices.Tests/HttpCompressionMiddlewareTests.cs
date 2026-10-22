using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Infrastructure.Middleware;

namespace VAA.EAPI.Microservices.Tests
{
    public class HttpCompressionMiddlewareTests
    {
        public HttpCompressionMiddlewareTests()
        {
            var bindingData = new Dictionary<string, object?>
            {
                { "Query", "{}" },
                { "Headers", "{\"Accept\":\"*/*\",\"Connection\":\"keep-alive\",\"Host\":\"localhost:7071\",\"User-Agent\":\"xUnit Test/7.42.0\",\"Accept-Encoding\":\"gzip, deflate, br\",\"Cache-Control\":\"no-cache\"}" }
            };

            var a = AppDomain.CurrentDomain.GetAssemblies()
                                           .Where(x => x.FullName.StartsWith("Microsoft.Azure.Functions.Worker"))
                                           .ToArray()
                                           .First()
                                           .GetTypes()
                                           .OrderBy(x => x.Name)
                                           .ToArray();

            //Microsoft.Azure.Functions.Worker.Grpc

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact(Skip = "Work in progress")]
        private async Task HttpCompression()
        {
            var compressionMiddleware = new HttpCompressionMiddleware();

            var httpRequestData = await AsyncContext.Current.Context.GetHttpRequestDataAsync();

            var a = AsyncContext.Current.Context.Features.Get<IHttpRequestDataFeature>();

            AsyncContext.Current.Context.SetHttpRequestData(httpRequestData);

            var functionExecutionDelegate = new FunctionExecutionDelegate(async (context) =>
            {




                await Task.Delay(1000);
            });

            await compressionMiddleware.Invoke(AsyncContext.Current.Context, functionExecutionDelegate);
        }
    }
}
