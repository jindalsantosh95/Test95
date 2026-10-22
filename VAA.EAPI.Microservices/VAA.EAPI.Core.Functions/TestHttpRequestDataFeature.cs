using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Core.Functions
{
    public sealed class TestHttpRequestDataFeature : IHttpRequestDataFeature
    {
        public static TestHttpRequestDataFeature Instance { get; } = new TestHttpRequestDataFeature();

        private const string HttpTriggerString = "httpTrigger";

        private TestHttpRequestDataFeature() { }

        public async ValueTask<HttpRequestData?> GetHttpRequestDataAsync(FunctionContext context)
        {
            var httpTriggerBinding = context.FunctionDefinition
                                            .InputBindings
                                            .Values
                                            .FirstOrDefault(a => HttpTriggerString.Is(a.Type));

            if (context.Features.Any(x => x.Key == typeof(HttpRequestData)))
            {
                HttpRequestData? httpRequestData = context.Features.Get<HttpRequestData>();

                return await new ValueTask<HttpRequestData?>(httpRequestData);
            }

            return httpTriggerBinding != null ? (await context.BindInputAsync<HttpRequestData>(httpTriggerBinding)).Value : default;
        }
    }
}
