using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Serialization;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Core.Functions;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Tests
{
    public sealed class GatewayQueryTests
    {
        [Theory]
        [InlineData("{ __typename }")]
        [InlineData("query Check { first: __typename second: __typename third: __typename }")]
        public async Task QueryWithoutArgumentsExecutesThroughHttpGatewayAsync(string query)
        {
            await using var provider = new ServiceCollection().AddConcierge().BuildServiceProvider();
            var resolver = provider.GetRequiredService<IRequestExecutorResolver>();
            var gateway = new Gateway(provider.GetRequiredService<IHttpRequestParser>(),
                provider.GetRequiredService<IHttpResultSerializer>(), resolver, NullLogger<Gateway>.Instance);
            var function = new global::VAA.EAPI.GraphQL.GraphQL(gateway, resolver,
                NullLogger<global::VAA.EAPI.GraphQL.GraphQL>.Instance);
            var context = new TestFunctionContext();
            var previous = AsyncContext.Current;
            AsyncContext.Current = new AsyncContext { Context = context };
            try
            {
                using var body = new MemoryStream(Encoding.UTF8.GetBytes(new JObject { ["query"] = query }.ToString()));
                var request = new TestHttpRequestData(context, new Uri("http://localhost/api/v1/GraphQL"), body, "POST");
                var response = await function.GetData(request, context);
                response.Body.Position = 0;
                var result = JObject.Parse(await new StreamReader(response.Body).ReadToEndAsync());
                Assert.Null(result["errors"]);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.All(((JObject)result["data"]!).Properties(), field => Assert.Equal("Query", field.Value.ToString()));
            }
            finally { AsyncContext.Current = previous; }
        }
    }
}
