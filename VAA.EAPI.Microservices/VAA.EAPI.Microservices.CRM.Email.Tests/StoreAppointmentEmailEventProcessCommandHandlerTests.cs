using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.StoreAppointment;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.CRM.Email.Tests
{
    public class StoreAppointmentEmailEventProcessCommandHandlerTests
    {
        private readonly StoreAppointmentEmailEventProcessCommandHandler _successfulHandler;
        private readonly StoreAppointmentEmailEventProcessCommandHandler _failingHandler;
        private readonly FakeAcmClient _successfulClient;
        private readonly FakeAcmClient _failingClient;

        public StoreAppointmentEmailEventProcessCommandHandlerTests()
        {
            ConfigReader.Set("retrycounter", "1");
            ConfigReader.Set("delaytime", "0:0:0:0");
            ConfigReader.Set("enableemailmsauthentication", "false");
            ConfigReader.Set("acmsessiontoken", "test_token");
            ConfigReader.Set("acmcommunicationchannel", "0");

            _successfulClient = new FakeAcmClient(true);
            _failingClient = new FakeAcmClient(false);

            _successfulHandler = new StoreAppointmentEmailEventProcessCommandHandler(NullLogger<StoreAppointmentEmailEventProcessCommandHandler>.Instance, _successfulClient);
            _failingHandler = new StoreAppointmentEmailEventProcessCommandHandler(NullLogger<StoreAppointmentEmailEventProcessCommandHandler>.Instance, _failingClient);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_ForSuccessfulRequest_AndPublishExpectedPayload()
        {
            var request = TestDataProvider.GetTestData<StoreAppointmentEmailEventProcessCommand>("EmailMS\\StoreApptEmailEventProcessCommandRequest");

            var result = await _successfulHandler.Handle(request, CancellationToken.None);

            Assert.True(result);
            Assert.Equal(1, _successfulClient.CallCount);
            Assert.NotNull(_successfulClient.LastRequest);
            Assert.Contains("type=\"vhoAppointmentConfirmationCustomer\"", _successfulClient.LastRequest!);
            Assert.Contains("email=\"customer@test.com\"", _successfulClient.LastRequest!);
            Assert.Contains("<ctx>", _successfulClient.LastRequest!);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenAcmClientFails()
        {
            var request = TestDataProvider.GetTestData<StoreAppointmentEmailEventProcessCommand>("EmailMS\\StoreApptEmailEventProcessCommandRequest");

            var result = await _failingHandler.Handle(request, CancellationToken.None);

            Assert.False(result);
            Assert.Equal(1, _failingClient.CallCount);
        }

        private sealed class FakeAcmClient : IACMClient
        {
            private readonly bool _result;
            public int CallCount { get; private set; }
            public string? LastRequest { get; private set; }

            public FakeAcmClient(bool result)
            {
                _result = result;
            }

            public Task<bool> SendEmail(string aCMRequest)
            {
                CallCount++;
                LastRequest = aCMRequest;
                return Task.FromResult(_result);
            }
        }
    }
}