using HotChocolate.Execution;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Middleware;
using Xunit;

namespace VAA.EAPI.GraphQL.Tests
{

    [Collection(nameof(AccountLinkingProofRequestMiddlewareTests))]
    public sealed class GatewayBatchBoundaryTests
    {
        [Theory]
        [InlineData("[{\"query\":\"query { rewardSeatAvailability }\"},{\"query\":\"query { rewardSeatAvailability }\"}]")]
        [InlineData("[{\"query\":\"query { status }\"},{\"query\":\"query { status }\"}]")]
        [InlineData(" \r\n[{\"query\":\"query { rewardSeatAvailability }\"}]")]
        [InlineData("[]")]
        public async Task GetData_ExistingSingleObjectBoundaryPreventsBatchExecution(string body)
        {
            await VerifyGatewayAsync(body, isBatch: true);
        }

        [Theory]
        [InlineData("status")]
        [InlineData("rewardSeatAvailability")]
        public async Task GetData_SingleRequestStillExecutesExactlyOnce(string field)
        {
            var body = JsonConvert.SerializeObject(new
            {
                query = $"query SingleRequest($include: Boolean! = true) {{ {field} @include(if: $include) }}"
            });

            await VerifyGatewayAsync(body, isBatch: false);
        }

        private static async Task VerifyGatewayAsync(string body, bool isBatch)
        {
            var executions = 0;
            var services = new ServiceCollection().AddLogging();
            services.AddGraphQLServer()
                .AddQueryType(descriptor =>
                {
                    descriptor.Name("Query");
                    descriptor.Field("status").Type<HotChocolate.Types.StringType>()
                        .Resolve(_ => ValueTask.FromResult<object?>(Interlocked.Increment(ref executions).ToString()));
                    descriptor.Field("rewardSeatAvailability").Type<HotChocolate.Types.StringType>()
                        .Resolve(_ => ValueTask.FromResult<object?>(Interlocked.Increment(ref executions).ToString()));
                })
                .UseDefaultPipeline()
                .UseRequest<OperationLimiterMiddleware>();
            services.AddSingleton<Gateway>().AddSingleton<global::VAA.EAPI.GraphQL.GraphQL>();
            using var provider = services.BuildServiceProvider();
            var context = new TestFunctionContext();
            AsyncContext.Current = new AsyncContext { Context = context };
            try
            {
                var request = new TestHttpRequestData(context, new Uri("https://graphql.example.test/api/v1/GraphQL"),
                    new MemoryStream(Encoding.UTF8.GetBytes(body)), "POST");
                var graphQL = provider.GetRequiredService<global::VAA.EAPI.GraphQL.GraphQL>();
                if (isBatch)
                {
                    // The original persisted-query normalizer requires a GraphQLModel object.
                    // Preserve its existing exception behaviour; do not add a gateway-wide HTTP contract.
                    await Assert.ThrowsAsync<JsonSerializationException>(() => graphQL.GetData(request, context));
                    Assert.Equal(0, executions);
                    return;
                }

                HttpResponseData response = await graphQL.GetData(request, context);
                response.Body.Position = 0;
                using var reader = new StreamReader(response.Body);
                var result = await reader.ReadToEndAsync();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.DoesNotContain("errors", result, StringComparison.Ordinal);
                Assert.Equal(1, executions);
            }
            finally
            {
                AsyncContext.Current = null;
            }
        }
    }
}
