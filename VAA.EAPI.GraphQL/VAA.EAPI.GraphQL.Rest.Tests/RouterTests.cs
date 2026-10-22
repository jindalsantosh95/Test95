using Microsoft.Extensions.DependencyInjection;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Newtonsoft.Json.Linq;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Rest.FunctionHost;
using Xunit;

namespace VAA.EAPI.GraphQL.Rest.Tests
{
    public sealed class RouterTests
    {
        private readonly IServiceProvider _serviceProvider;

        public RouterTests()
        {
            var services = new ServiceCollection();

            _serviceProvider = services.AddLogging()
                                       .AddSingleton<IHttpClientFactory, VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory>()
                                       .AddSingleton<IHttpConnector, HttpConnector>()
                                       .AddSingleton<IServiceCollection>(services)
                                       .AddSingleton<VersionQuery>()
                                       .AddSingleton<VaaNotFoundQuery>()
                                       .AddSingleton<Router>()
                                       .AddSingleton<RouterFunction>()
                                       .BuildServiceProvider();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public async Task Route_ReturnsNotFound_ForUnknownRoute()
        {
            // Arrange
            var router = _serviceProvider.GetRequiredService<Router>();

            var testContext = new TestFunctionContext([]);
            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/api/unknown"), method: "GET");

            testContext.Features.Set(httpRequestData);

            // Act
            var response = await router.Route(httpRequestData, "unknown");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Route_ReturnsOk_ForKnownMapping()
        {
            // Arrange
            var router = _serviceProvider.GetRequiredService<Router>();

            var testContext = new TestFunctionContext([]);
            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/api/v1/version"), method: "GET");

            testContext.Features.Set(httpRequestData);

            // Act
            var mapping = router.Routes.SelectMany(r => r.Value).FirstOrDefault();

            Assert.NotNull(mapping);
            Assert.Equal("version", mapping.Name);

            var response = await router.Route(httpRequestData, mapping.Name);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // cast response.Body as memory stream and read as string
            var memoryStream = (MemoryStream)response.Body;

            memoryStream.Position = 0;

            var content = new StreamContent(memoryStream);

            Assert.True(memoryStream.CanSeek);

            string payload = await content.ReadAsStringAsync();

            JObject instance = Json.Deserialize<JObject>(payload);

            Assert.NotNull(instance);
            Assert.True(instance.ContainsKey("major"));
            Assert.True(instance.ContainsKey("minor"));
            Assert.True(instance.ContainsKey("build"));
        }

        [Fact]
        public async Task Route_ReturnsCapabilityIndex_ForOptionsWithoutRouteName()
        {
            // Arrange
            var router = _serviceProvider.GetRequiredService<Router>();

            var testContext = new TestFunctionContext([]);
            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/api/v1"), method: "OPTIONS");

            testContext.Features.Set(httpRequestData);

            // Act
            var response = await router.Route(httpRequestData, null);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("text/markdown", response.Headers.GetValues("content-type"));

            var memoryStream = (MemoryStream)response.Body;

            memoryStream.Position = 0;

            var payload = await new StreamContent(memoryStream).ReadAsStringAsync();

            Assert.Contains("| Route | Method |", payload);
            Assert.Contains("| [/api/v1/version](/api/v1/version) | [OPTIONS, GET] |", payload);
        }

        [Fact]
        public async Task Route_ReturnsNotFound_WhenResolverReportsConfiguredNotFoundError()
        {
            var router = _serviceProvider.GetRequiredService<Router>();
            var testContext = new TestFunctionContext([]);
            var request = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/api/v1/notFoundResult"), method: "GET");

            testContext.Features.Set(request);

            var response = await router.Route(request, "notFoundResult");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var payload = await ReadJsonResponse(response);
            var error = payload["errors"]?[0];

            Assert.Equal("No result was found.", error?["message"]?.Value<string>());
            Assert.Equal("101668", error?["extensions"]?["code"]?.Value<string>());
        }

        [Fact]
        public async Task Route_ReturnsNotFound_WhenResolverReportsHttpNotFoundCode()
        {
            var response = await Route("httpNotFoundCodeResult");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var payload = await ReadJsonResponse(response);
            Assert.Equal("404", payload["errors"]?[0]?["extensions"]?["code"]?.Value<string>());
        }

        [Theory]
        [InlineData("noDataFoundResult")]
        [InlineData("noBookingsFoundResult")]
        [InlineData("noMealOptionsResult")]
        [InlineData("noSeatOffersResult")]
        [InlineData("notFoundExtensionCodeResult")]
        [InlineData("pnrNotFoundResult")]
        public async Task Route_ReturnsNotFound_ForQueryNotFoundMessagesAndExtensionCodes(string routeName)
        {
            var response = await Route(routeName);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("thrownNotFoundResult", "No result was found.")]
        [InlineData("nullResult", "No result was returned.")]
        public async Task Route_ReturnsNotFound_WhenQueryThrowsConfiguredNotFoundOrReturnsNull(string routeName, string expectedMessage)
        {
            var response = await Route(routeName);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var payload = await ReadJsonResponse(response);
            var error = payload["errors"]?[0];

            Assert.Equal(expectedMessage, error?["message"]?.Value<string>());
            Assert.Equal("404", error?["extensions"]?["code"]?.Value<string>());
        }

        [Theory]
        [InlineData("straightApostropheNotFoundMessageResult")]
        [InlineData("curlyApostropheNotFoundMessageResult")]
        [InlineData("innerNotFoundMessageResult")]
        [InlineData("httpRequestNotFoundResult")]
        [InlineData("httpResponseNotFoundResult")]
        public async Task Route_ReturnsNotFound_ForSupportedExceptionShapes(string routeName)
        {
            var response = await Route(routeName);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("unmatchedInvalidOperationResult", HttpStatusCode.InternalServerError)]
        [InlineData("unmatchedExceptionResult", HttpStatusCode.InternalServerError)]
        [InlineData("httpRequestFailureResult", HttpStatusCode.InternalServerError)]
        [InlineData("validationDoesNotExistResult", HttpStatusCode.InternalServerError)]
        [InlineData("notFoundMessageWith500CodeResult", HttpStatusCode.InternalServerError)]
        [InlineData("couldntAddWith422CodeResult", HttpStatusCode.UnprocessableEntity)]
        [InlineData("thrownGraphQL422Result", HttpStatusCode.UnprocessableEntity)]
        [InlineData("mixed404Then422Result", HttpStatusCode.UnprocessableEntity)]
        [InlineData("mixed422Then404Result", HttpStatusCode.UnprocessableEntity)]
        public async Task Route_DoesNotReturnNotFound_ForUnmatchedExceptions(string routeName, HttpStatusCode expectedStatusCode)
        {
            var response = await Route(routeName);

            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory]
        [InlineData(400)]
        [InlineData(401)]
        [InlineData(403)]
        [InlineData(503)]
        public async Task ExplicitTransportErrorsPreserveDomainEnvelope(int status)
        {
            var router = _serviceProvider.GetRequiredService<Router>();
            var context = new TestFunctionContext([]);
            var request = new TestHttpRequestData(context, new Uri("http://tempuri.org/api/v1/transportError"), method: "POST");
            using (var body = new StreamWriter(request.Body, leaveOpen: true))
            {
                await body.WriteAsync(Json.Serialize(new { status }));
                await body.FlushAsync();
            }
            request.Body.Position = 0;
            var response = await router.Route(request, "transportError");
            Assert.Equal((HttpStatusCode)status, response.StatusCode);
            var payload = await ReadJsonResponse(response);
            Assert.Equal("DOMAIN_FAILURE", payload["errors"]?[0]?["extensions"]?["code"]?.Value<string>());
            Assert.Equal(status == 503, payload["errors"]?[0]?["extensions"]?["retryable"]?.Value<bool>());
            Assert.Equal("test-correlation", payload["errors"]?[0]?["extensions"]?["correlationId"]?.Value<string>());

            var mcpRequest = new TestHttpRequestData(context, new Uri("http://tempuri.org/api/mcp"), method: "POST");
            using var writer = new StreamWriter(mcpRequest.Body, leaveOpen: true);
            await writer.WriteAsync(Json.Serialize(new { jsonrpc = "2.0", id = 1, method = "tools/call", @params = new { name = "transportError", arguments = new { status } } }));
            await writer.FlushAsync();
            mcpRequest.Body.Position = 0;
            var mcp = await router.Mcp(mcpRequest);
            Assert.Equal(status, mcp.Error.Code);
            var error = mcp.Error.Data["errors"]![0]!;
            Assert.Equal("DOMAIN_FAILURE", error["extensions"]!["code"]!.Value<string>());
            Assert.Equal(status == 503, error["extensions"]!["retryable"]!.Value<bool>());
            Assert.Equal("test-correlation", error["extensions"]!["correlationId"]!.Value<string>());
        }

        [Fact]
        public async Task McpKeepsLegacyGraphQLErrorBehaviorWithoutTransportOptIn()
        {
            var context = new TestFunctionContext([]);
            var request = new TestHttpRequestData(context, new Uri("http://tempuri.org/api/mcp"), method: "POST");
            using var writer = new StreamWriter(request.Body, leaveOpen: true);
            await writer.WriteAsync("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/call\",\"params\":{\"name\":\"thrownGraphQL422Result\",\"arguments\":{}}}");
            await writer.FlushAsync();
            request.Body.Position = 0;
            var response = await _serviceProvider.GetRequiredService<Router>().Mcp(request);
            Assert.Equal(500, response.Error.Code);
        }

        [Theory]
        [InlineData("unauthorizedResult", HttpStatusCode.Unauthorized)]
        [InlineData("forbiddenResult", HttpStatusCode.Forbidden)]
        public async Task AccessFailuresPreserveRestAndMcpStatus(string routeName, HttpStatusCode status)
        {
            var response = await Route(routeName);
            Assert.Equal(status, response.StatusCode);
            response.Body.Position = 0;
            Assert.Equal(status.ToString(), await new StreamReader(response.Body).ReadToEndAsync());

            var context = new TestFunctionContext([]);
            var request = new TestHttpRequestData(context, new Uri("http://tempuri.org/api/mcp"), method: "POST");
            using var writer = new StreamWriter(request.Body, leaveOpen: true);
            await writer.WriteAsync(Json.Serialize(new { jsonrpc = "2.0", id = 1, method = "tools/call", @params = new { name = routeName, arguments = new { } } }));
            await writer.FlushAsync();
            request.Body.Position = 0;
            var mcp = await _serviceProvider.GetRequiredService<Router>().Mcp(request);
            Assert.Equal((int)status, mcp.Error.Code);
            Assert.Equal(status.ToString(), mcp.Error.Message);
        }

        private async Task<Microsoft.Azure.Functions.Worker.Http.HttpResponseData> Route(string routeName)
        {
            var router = _serviceProvider.GetRequiredService<Router>();
            var testContext = new TestFunctionContext([]);
            var request = new TestHttpRequestData(testContext, new Uri($"http://tempuri.org/api/v1/{routeName}"), method: "GET");

            testContext.Features.Set(request);

            return await router.Route(request, routeName);
        }

        private static async Task<JObject> ReadJsonResponse(Microsoft.Azure.Functions.Worker.Http.HttpResponseData response)
        {
            response.Body.Position = 0;
            var payload = await new StreamContent(response.Body).ReadAsStringAsync();

            return Json.Deserialize<JObject>(payload);
        }
    }

    [ExtendObjectType("Query")]
    public sealed class VaaNotFoundQuery
    {
        [GraphQLName("unauthorizedResult")]
        public Task<string> GetUnauthorizedResult() => Task.FromException<string>(new UnauthorizedAccessException("Unauthorized"));

        [GraphQLName("forbiddenResult")]
        public Task<string> GetForbiddenResult() => Task.FromException<string>(new UnauthorizedAccessException("Forbidden"));

        [GraphQLName("transportError")]
        public string GetTransportError(int status) => throw new GraphQLException(ErrorBuilder.New()
            .SetMessage("Controlled failure.").SetCode("DOMAIN_FAILURE")
            .SetExtension("httpStatusCode", status).SetExtension("transportStatusCode", status)
            .SetExtension("retryable", status == 503).SetExtension("correlationId", "test-correlation").Build());

        [GraphQLName("notFoundResult")]
        public string GetNotFoundResult(IResolverContext resolverContext)
        {
            resolverContext.ReportError(ErrorBuilder.New()
                .SetMessage("No result was found.")
                .SetCode("101668")
                .Build());

            return "ignored";
        }

        [GraphQLName("httpNotFoundCodeResult")]
        public string GetHttpNotFoundCodeResult(IResolverContext resolverContext)
        {
            resolverContext.ReportError(ErrorBuilder.New()
                .SetMessage("The requested item is unavailable.")
                .SetCode("404")
                .Build());

            return "ignored";
        }

        [GraphQLName("noDataFoundResult")]
        public string GetNoDataFoundResult() => throw new InvalidOperationException("No data found for the given phone number.");

        [GraphQLName("noBookingsFoundResult")]
        public string GetNoBookingsFoundResult(IResolverContext resolverContext) => ReportError(resolverContext, "No bookings found for the provided FC Number.", "NO_BOOKINGS_FOUND");

        [GraphQLName("noMealOptionsResult")]
        public string GetNoMealOptionsResult(IResolverContext resolverContext) => ReportError(resolverContext, "No meal options available for this flight / cabin combination.", "NO_SSRS_FOUND");

        [GraphQLName("noSeatOffersResult")]
        public string GetNoSeatOffersResult() => throw new InvalidOperationException("No SeatOffers Found");

        [GraphQLName("notFoundExtensionCodeResult")]
        public string GetNotFoundExtensionCodeResult(IResolverContext resolverContext) => ReportError(resolverContext, "The requested item is unavailable.", "404");

        [GraphQLName("pnrNotFoundResult")]
        public string GetPnrNotFoundResult(IResolverContext resolverContext) => ReportError(resolverContext, "PNR not found with given details.", "404");

        [GraphQLName("notFoundMessageWith500CodeResult")]
        public string GetNotFoundMessageWith500CodeResult(IResolverContext resolverContext) => ReportError(resolverContext, "PNR not found with given details.", "500");

        [GraphQLName("couldntAddWith422CodeResult")]
        public string GetCouldntAddWith422CodeResult(IResolverContext resolverContext) => ReportError(resolverContext, "We couldn’t add your trip at the moment.", "422");

        [GraphQLName("thrownGraphQL422Result")]
        public string GetThrownGraphQL422Result() => throw new GraphQLException(ErrorBuilder.New()
            .SetMessage("We couldn’t add your trip at the moment. Please try again later.")
            .SetCode("422")
            .Build());

        [GraphQLName("mixed404Then422Result")]
        public string GetMixed404Then422Result(IResolverContext resolverContext)
        {
            resolverContext.ReportError(ErrorBuilder.New().SetMessage("PNR not found.").SetCode("404").Build());
            return ReportError(resolverContext, "The request is invalid.", "422");
        }

        [GraphQLName("mixed422Then404Result")]
        public string GetMixed422Then404Result(IResolverContext resolverContext)
        {
            resolverContext.ReportError(ErrorBuilder.New().SetMessage("The request is invalid.").SetCode("422").Build());
            return ReportError(resolverContext, "PNR not found.", "404");
        }

        [GraphQLName("thrownNotFoundResult")]
        public string GetThrownNotFoundResult() => throw new InvalidOperationException("101668");

        [GraphQLName("straightApostropheNotFoundMessageResult")]
        public string GetStraightApostropheNotFoundMessageResult() => throw new InvalidOperationException("The service couldn't find the requested item.");

        [GraphQLName("curlyApostropheNotFoundMessageResult")]
        public string GetCurlyApostropheNotFoundMessageResult() => throw new InvalidOperationException("The service couldn’t find the requested item.");

        [GraphQLName("innerNotFoundMessageResult")]
        public string GetInnerNotFoundMessageResult() => throw new InvalidOperationException("Outer failure.", new Exception("The requested item does not exist."));

        [GraphQLName("httpRequestNotFoundResult")]
        public string GetHttpRequestNotFoundResult() => throw new HttpRequestException("Downstream failure.", null, HttpStatusCode.NotFound);

        [GraphQLName("httpResponseNotFoundResult")]
        public string GetHttpResponseNotFoundResult() => throw new Common.Exceptions.HttpResponseException(HttpStatusCode.NotFound);

        [GraphQLName("unmatchedInvalidOperationResult")]
        public string GetUnmatchedInvalidOperationResult() => throw new InvalidOperationException("The item could not be added.");

        [GraphQLName("unmatchedExceptionResult")]
        public string GetUnmatchedExceptionResult() => throw new Exception("Unexpected downstream failure.");

        [GraphQLName("validationDoesNotExistResult")]
        public string GetValidationDoesNotExistResult() => throw new Common.Exceptions.RequestValidationException("Either 'Partner' or 'Product' does not exist in the system.");

        [GraphQLName("httpRequestFailureResult")]
        public string GetHttpRequestFailureResult() => throw new HttpRequestException("Downstream failure.", null, HttpStatusCode.InternalServerError);

        [GraphQLName("nullResult")]
        public string? GetNullResult() => null;

        private static string ReportError(IResolverContext resolverContext, string message, string code)
        {
            resolverContext.ReportError(ErrorBuilder.New()
                .SetMessage(message)
                .SetExtension("code", code)
                .Build());

            return "ignored";
        }
    }

}
