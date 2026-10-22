using MediatR;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http;
using Xunit;
using Preference = VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount.Preference;
using PreferenceTypes = VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount.PreferenceTypes;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class CustomerProfileControllerTest
    {
        private const string DefaultCustomerProfileDocType = "CustomerProfile";

        private readonly Mock<IMediator> _mediator;
        private readonly TestFunctionContext _context;
        private readonly CustomerProfileController _controller;

        public CustomerProfileControllerTest()
        {
            _mediator = new Mock<IMediator>();
            _context = new TestFunctionContext();
            _controller = new CustomerProfileController(_mediator.Object, new TestCustomerProfileUpdatePreferencesCommand());
            ConfigReader.Set(EnvironmentSettings.CustomerProfileDocType, DefaultCustomerProfileDocType);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileController(null, new TestCustomerProfileUpdatePreferencesCommand()));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCustomerProfileUpdatePreferencesCommandIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileController(_mediator.Object, null));
        }

        [Fact]
        public async Task CustomerProfileDocumentByCustomerIdAsync_Success()
        {
            string body = "";
            var req = SetupTestDataRequests(body);
            _mediator.Setup(m => m.Send(It.IsAny<CustomerProfileQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<CustomerProfileDocResponse> { });
            var Output = await _controller.CustomerProfileDocumentByCustomerIdAsync(req.Object, "D4718738-BADA-E0ED-E053-6941D90AAD4B");
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.StatusCode);
        }

        private Mock<HttpRequestData> SetupTestDataRequests(string requestBody)
        {
            string body = requestBody;
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);
            var req = new Mock<HttpRequestData>(_context);
            req.Setup(m => m.Body).Returns(bodyStream);
            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                return response.Object;
            });
            return req;
        }

        [Fact]
        public async Task UpdateCustomerPreferences_WhenValidBody_ReturnsCommandResponse_AndMapsCommand()
        {
            var requestBody = new UpdateCustomerProfilePreference
            {
                CustomerId = "CUST-456",
                FcNumber = "FC-123123",
                preference = [new Preference { PreferenceType = PreferenceTypes.PostCommunication, PreferenceValue = "false" }],
                EmailAddress = "another@test.local",
                FirstName = "Another",
                TierCode = "S",
                LastUpdatedBy = "test"
            };

            var expected = new UpdateMarketingPreferenceResponse { PreferenceValue = true };

            var updatePreferencesCommand = new CapturingCustomerProfileUpdatePreferencesCommand
            {
                ResponseToReturn = expected
            };

            var controller = new CustomerProfileController(_mediator.Object, updatePreferencesCommand);
            var req = CreateHttpRequest(method: "PATCH", bodyJson: Json.Serialize(requestBody, ignoreNullValues: false));

            var result = await controller.UpdateCustomerPreferences(req);

            Assert.True(result.PreferenceValue);
            Assert.NotNull(updatePreferencesCommand.LastRequest);
            Assert.Equal(requestBody.CustomerId, updatePreferencesCommand.LastRequest!.CustomerId);
            Assert.Equal(requestBody.FcNumber, updatePreferencesCommand.LastRequest.FcNumber);
        }

        private TestHttpRequestData CreateHttpRequest(string method, string? bodyJson = null)
        {
            Stream? bodyStream = null;

            if (bodyJson is not null)
                bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(bodyJson));

            return new TestHttpRequestData(_context, new Uri("http://tempuri.org/"), bodyStream, method);
        }

        private sealed class TestCustomerProfileUpdatePreferencesCommand : ICustomerProfileUpdatePreferencesCommand
        {
            public Task<UpdateMarketingPreferenceResponse> Handle(CustomerProfileCommand request, CancellationToken cancellationToken) =>
                Task.FromResult(new UpdateMarketingPreferenceResponse());
        }

        private sealed class CapturingCustomerProfileUpdatePreferencesCommand : ICustomerProfileUpdatePreferencesCommand
        {
            public CustomerProfileCommand? LastRequest { get; private set; }
            public UpdateMarketingPreferenceResponse ResponseToReturn { get; set; } = new();

            public Task<UpdateMarketingPreferenceResponse> Handle(CustomerProfileCommand request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Task.FromResult(ResponseToReturn);
            }
        }
    }
}
