using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

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
                                            .FirstOrDefault(a => string.Equals(a.Type, HttpTriggerString, StringComparison.OrdinalIgnoreCase));

            return httpTriggerBinding != null ? (await context.BindInputAsync<HttpRequestData>(httpTriggerBinding)).Value : default;
        }
    }
}
