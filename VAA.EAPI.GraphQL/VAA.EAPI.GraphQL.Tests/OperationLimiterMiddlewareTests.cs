using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Middleware;
using Xunit;

namespace VAA.EAPI.GraphQL.Tests
{
    public sealed class OperationLimiterMiddlewareTests
    {
        [Theory]
        [InlineData("query { a: status b: status c: status d: status e: status }", false)]
        [InlineData("query { a: status b: status c: status d: status e: status f: status }", true)]
        public async Task PreservesExistingGeneralOperationLimit(string query, bool rejected)
        {
            var services = new ServiceCollection();
            services.AddGraphQLServer()
                .AddQueryType(descriptor =>
                {
                    descriptor.Name("Query");
                    descriptor.Field("status").Type<HotChocolate.Types.StringType>()
                        .Resolve(_ => ValueTask.FromResult<object?>("ok"));
                })
                .UseDefaultPipeline()
                .UseRequest<OperationLimiterMiddleware>();
            using var provider = services.BuildServiceProvider();
            var executor = await provider.GetRequiredService<IRequestExecutorResolver>().GetRequestExecutorAsync();
            var request = QueryRequestBuilder.New().SetQuery(query).Create();

            var result = await executor.ExecuteAsync(request);
            var json = result.ToJson();

            Assert.Equal(rejected, json.Contains("Only 5 operation(s)", StringComparison.Ordinal));
            if (!rejected)
                Assert.DoesNotContain("errors", json, StringComparison.OrdinalIgnoreCase);
        }
    }
}
