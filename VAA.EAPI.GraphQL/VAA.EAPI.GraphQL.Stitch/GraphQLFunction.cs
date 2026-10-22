using HotChocolate.AzureFunctions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using VAA.EAPI.GraphQL.Stitch.Core;

namespace VAA.EAPI.GraphQL.Stitch
{
    public sealed class GraphQLFunction(IGraphQLRequestExecutor executor)
    {
        private readonly IGraphQLRequestExecutor _executor = executor;

        [Function("GetData")]
        public async Task<HttpResponseData> GetData([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "v1/graphql/{**slug}")] HttpRequestData request)
        {
            var response = await _executor.ExecuteAsync(request);

            AppendStitchedResponseHeaders(response);

            if (response.StatusCode == System.Net.HttpStatusCode.OK) return response;
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) return response;

            var stream = (MemoryStream)response.Body;

            if (stream.Length > 0)
            {
                stream.Position = 0;

                var first = (char)(byte)stream.ReadByte();

                if (first == '{')
                    response.StatusCode = System.Net.HttpStatusCode.OK;

                stream.Position = stream.Length;
            }

            return response;
        }

        private static void AppendStitchedResponseHeaders(HttpResponseData response)
        {
            var context = AsyncContext.Current?.Context;

            if (context is null) return;
            if (!context.Items.TryGetValue(Core.Constants.Application.ResponseHeaderKey, out var item)) return;
            if (item is not Dictionary<string, List<string>> responseHeaders) return;

            foreach (var header in responseHeaders)
            {
                if (string.IsNullOrWhiteSpace(header.Key) || header.Key.Contains('\r') || header.Key.Contains('\n')) continue;

                var values = header.Value
                    .Where(value => !string.IsNullOrWhiteSpace(value) && !value.Contains('\r') && !value.Contains('\n'))
                    .ToArray();

                if (values.Length == 0) continue;

                response.Headers.TryAddWithoutValidation(header.Key, values);
            }
        }
    }
}
