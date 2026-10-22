using System.Net;
using System.Net.Http;
using System.Text;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Rest.FunctionHost;
using Xunit;
using XunitHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.GraphQL.Rest.Tests
{
    [Collection(nameof(AddressLookupV3RoutingTests))]
    public sealed class AddressLookupV3RoutingTests
    {
        [Theory]
        [InlineData(false, "mixed-query", 400)]
        [InlineData(false, "mixed-reference", 400)]
        [InlineData(false, "missing-key", 503)]
        [InlineData(true, "missing-key", 503)]
        public async Task ValidationFailures_PreserveStatusInGraphQLAndRest_WithoutDispatch(bool selection, string scenario, int status)
        {
            const string endpoint = "https://address-validation.example.test/api";
            var settings = new Dictionary<string, string>
            {
                [Domain.Account.Constants.AddressLookupV3Settings.AddressLookupV3ApimUrl] = endpoint,
                [Common.Constants.Environment.EnrolmentMSOcpKey] = scenario == "missing-key" ? "" : "test-enrolment-key"
            };
            var previousSettings = settings.Keys.ToDictionary(key => key, key => ConfigReader.Try(key));
            try
            {
                foreach (var setting in settings)
                    ConfigReader.Set(setting.Key, setting.Value);

                AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
                var services = new ServiceCollection().AddLogging()
                    .AddSingleton<IHttpClientFactory, XunitHttpClientFactory>()
                    .AddSingleton<IHttpConnector, HttpConnector>()
                    .AddSingleton<IOAuthTokenProvider, NoDispatchTokenProvider>()
                    .AddSingleton<IAddressLookupV3Provider, AddressLookupV3Provider>()
                    .AddSingleton<AddressLookupV3Query>()
                    .AddSingleton<AddressLookupV3Mutation>();
                services.AddGraphQLCore();
                using var provider = services.BuildServiceProvider();
                var executor = SchemaBuilder.New().AddServices(provider)
                    .AddQueryType(descriptor => descriptor.Name("Query"))
                    .AddMutationType(descriptor => descriptor.Name("Mutation"))
                    .AddType<AddressLookupV3Query>().AddType<AddressLookupV3Mutation>()
                    .Create().MakeExecutable();

                var input = selection ? new JObject { ["reference"] = "opaque-address-reference" }
                    : new JObject { ["query"] = "10 Downing Street", ["countryCode"] = "GB" };
                if (scenario == "mixed-query")
                    input["postCode"] = "SW1A 2AA";
                else if (scenario == "mixed-reference")
                    input = new JObject { ["reference"] = "opaque-container-reference", ["addressLine1"] = "10" };

                var operation = selection ? "accountAddressSelectionV3" : "accountAddressSuggestionsV3";
                var inputType = selection ? "AddressLookupV3SelectionInput" : "AddressLookupV3SuggestionInput";
                var query = $"{(selection ? "mutation" : "query")}($request: {inputType}) {{ {operation}(request: $request) {{ flowId }} }}";
                var result = await executor.ExecuteAsync(QueryRequestBuilder.New().SetQuery(query).SetServices(provider)
                    .SetVariableValue("request", input.ToObject<Dictionary<string, object>>()).Create());
                AssertError(JObject.Parse(result.ToJson()), status);

                var router = new Router(services, provider);
                var context = new TestFunctionContext();
                using var body = new MemoryStream(Encoding.UTF8.GetBytes(new JObject { ["request"] = input }.ToString()));
                var request = new TestHttpRequestData(context, new Uri($"http://localhost/api/v1/{operation}"), body, "POST");
                context.Features.Set(request);
                var response = await router.Route(request, operation);
                Assert.Equal((HttpStatusCode)status, response.StatusCode);
                response.Body.Position = 0;
                using var reader = new StreamReader(response.Body);
                AssertError(JObject.Parse(await reader.ReadToEndAsync()), status);
                var factory = (XunitHttpClientFactory)provider.GetRequiredService<IHttpClientFactory>();
                Assert.Empty(factory.GetResponses(endpoint));
            }
            finally
            {
                foreach (var setting in previousSettings)
                    ConfigReader.Set(setting.Key, setting.Value);
            }
        }

        private sealed class NoDispatchTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) =>
                throw new InvalidOperationException("Invalid input must be rejected before acquiring an application token.");
        }

        private static void AssertError(JObject payload, int status)
        {
            var error = Assert.Single(Assert.IsType<JArray>(payload["errors"]));
            Assert.Equal(status == 400
                ? "Address fields cannot be combined with a query or container reference."
                : "Address Lookup v3 is temporarily unavailable.", error["message"]?.Value<string>());
            Assert.Equal(status.ToString(System.Globalization.CultureInfo.InvariantCulture), error["extensions"]?["code"]?.Value<string>());
            Assert.Equal(status, error["extensions"]?["httpStatusCode"]?.Value<int>());
            Assert.Equal(status, error["extensions"]?["transportStatusCode"]?.Value<int>());
        }

        [Theory]
        [InlineData("accountAddressSuggestionsV3")]
        [InlineData("accountAddressSelectionV3")]
        public void ExistingRouterDiscoversV3Operations(string operationName)
        {
            var services = new ServiceCollection()
                .AddSingleton<AddressLookupV3Query>()
                .AddSingleton<AddressLookupV3Mutation>();
            using var provider = services.BuildServiceProvider();

            var router = new Router(services, provider);

            Assert.True(router.Routes.TryGetValue(HttpMethod.Post, out var routes));
            var route = Assert.Single(routes, item => item.Name == operationName);
            Assert.Equal(operationName, route.Name);
        }
    }

    [CollectionDefinition(nameof(AddressLookupV3RoutingTests), DisableParallelization = true)]
    public sealed class AddressLookupV3RoutingCollection;
}
