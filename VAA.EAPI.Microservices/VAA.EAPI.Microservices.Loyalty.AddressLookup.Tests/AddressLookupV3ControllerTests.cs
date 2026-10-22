using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public sealed class AddressLookupV3ControllerTests
    {
        [Theory]
        [InlineData(nameof(AddressLookupV3Controller.AddressSuggestionsV3))]
        [InlineData(nameof(AddressLookupV3Controller.AddressSelectionsV3))]
        public void Operations_DeclareTheSameAuthenticationAsV2(string operation)
        {
            var v2 = typeof(AddressDetailsController).GetMethod(nameof(AddressDetailsController.AddressDetailsV2))!;
            var v3 = typeof(AddressLookupV3Controller).GetMethod(operation)!;
            var expected = Assert.Single(v2.GetCustomAttributes<OpenApiSecurityAttribute>());
            var actual = Assert.Single(v3.GetCustomAttributes<OpenApiSecurityAttribute>());
            Assert.Equal(expected.SchemeName, actual.SchemeName);
            Assert.Equal(expected.SchemeType, actual.SchemeType);
            Assert.Equal(expected.Flows, actual.Flows);
            Assert.Equal(v2.GetParameters()[0].GetCustomAttribute<HttpTriggerAttribute>()!.AuthLevel,
                v3.GetParameters()[0].GetCustomAttribute<HttpTriggerAttribute>()!.AuthLevel);
        }

        [Fact]
        public async Task AddressSuggestionsV3_WhenBodyIsEmpty_ReturnsInvalidSuggestionRequestBeforeDispatch()
        {
            var service = new TestAddressLookupV3Service();
            var controller = new AddressLookupV3Controller(service);
            var request = CreateRequest(string.Empty);

            var response = await controller.AddressSuggestionsV3(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, service.SuggestionCalls);
            Assert.Contains("InvalidSuggestionRequest", await ReadBodyAsync(response));
        }

        [Fact]
        public async Task AddressSelectionsV3_WhenBodyIsMalformed_ReturnsInvalidSelectionRequestBeforeDispatch()
        {
            var service = new TestAddressLookupV3Service();
            var controller = new AddressLookupV3Controller(service);
            var request = CreateRequest("{");

            var response = await controller.AddressSelectionsV3(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, service.SelectionCalls);
            Assert.Contains("InvalidSelectionRequest", await ReadBodyAsync(response));
        }

        [Fact]
        public async Task AddressSuggestionsV3_WhenRequestIsValid_DelegatesToTheV3Service()
        {
            var service = new TestAddressLookupV3Service
            {
                SuggestionResponse = new AddressLookupV3SuggestionResponse { FlowId = "flow-id", Candidates = [] }
            };
            var controller = new AddressLookupV3Controller(service);
            var request = CreateRequest("{\"query\":\"London\",\"countryCode\":\"GB\"}");

            var response = await controller.AddressSuggestionsV3(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, service.SuggestionCalls);
            Assert.Contains("flow-id", await ReadBodyAsync(response));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Cancellation_ReturnsDocumentedRequestTimeout(bool selection)
        {
            var service = new TestAddressLookupV3Service { Exception = new OperationCanceledException() };
            var controller = new AddressLookupV3Controller(service);
            var request = CreateRequest(selection ? "{\"reference\":\"test-reference\"}" : "{\"query\":\"London\",\"countryCode\":\"GB\"}");

            var response = selection
                ? await controller.AddressSelectionsV3(request, TestContext.Current.CancellationToken)
                : await controller.AddressSuggestionsV3(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.RequestTimeout, response.StatusCode);
            Assert.Contains("Cancelled", await ReadBodyAsync(response));
            Assert.Equal(1, service.SelectionCalls + service.SuggestionCalls);
            var method = typeof(AddressLookupV3Controller).GetMethod(selection
                ? nameof(AddressLookupV3Controller.AddressSelectionsV3)
                : nameof(AddressLookupV3Controller.AddressSuggestionsV3));
            Assert.NotNull(method);
            var metadata = Assert.Single(method.GetCustomAttributes<OpenApiResponseWithBodyAttribute>(), attribute => attribute.StatusCode == HttpStatusCode.RequestTimeout);
            Assert.Equal(typeof(AddressLookupV3ErrorResponse), metadata.BodyType);
            Assert.Equal("application/json", metadata.ContentType);
        }

        private static TestHttpRequestData CreateRequest(string body)
        {
            var context = new TestFunctionContext();
            context.InstanceServices = new ServiceCollection()
                .Configure<WorkerOptions>(options => options.Serializer = new JsonObjectSerializer())
                .BuildServiceProvider();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));

            return new TestHttpRequestData(context, new Uri("https://addresslookup.example.test/v3"), stream, "POST");
        }

        private static async Task<string> ReadBodyAsync(Microsoft.Azure.Functions.Worker.Http.HttpResponseData response)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body, leaveOpen: true);

            return await reader.ReadToEndAsync();
        }

        private sealed class TestAddressLookupV3Service : IAddressLookupV3Service
        {
            public Exception? Exception { get; set; }
            public int SelectionCalls { get; private set; }
            public int SuggestionCalls { get; private set; }
            public AddressLookupV3SuggestionResponse SuggestionResponse { get; set; }
            public AddressLookupV3SelectionResponse SelectionResponse { get; set; }

            public Task<AddressLookupV3SuggestionResponse> GetSuggestionsAsync(AddressLookupV3SuggestionRequest request, CancellationToken cancellationToken)
            {
                SuggestionCalls++;
                if (Exception is not null) return Task.FromException<AddressLookupV3SuggestionResponse>(Exception);

                return SuggestionResponse is null
                    ? Task.FromException<AddressLookupV3SuggestionResponse>(new InvalidOperationException("The service must not be called for an invalid request."))
                    : Task.FromResult(SuggestionResponse);
            }

            public Task<AddressLookupV3SelectionResponse> GetSelectionAsync(AddressLookupV3SelectionRequest request, CancellationToken cancellationToken)
            {
                SelectionCalls++;
                if (Exception is not null) return Task.FromException<AddressLookupV3SelectionResponse>(Exception);

                return SelectionResponse is null
                    ? Task.FromException<AddressLookupV3SelectionResponse>(new InvalidOperationException("The service must not be called for an invalid request."))
                    : Task.FromResult(SelectionResponse);
            }
        }

    }
}
