using Microsoft.Extensions.Logging;
using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services;
using VAA.Facade.NDC.IFGFacade.Application.Command;
using VAA.Facade.NDC.IFGFacade.Domain;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Mapper.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.ReferenceMS;
using static VAA.Facade.Test.Framework.TestHelpers;

namespace VAA.Facade.NDC.IFGFacade.Tests
{
    public class AuthorizeCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly IPaymentMSAuthorizeCommand _paymentMSAuthorizeCommand;
        private readonly IAirportDetailsQuery _airportDetailsQuery;
        private readonly ICurrencyDetailsQuery _currencyDetailsQuery;
        private readonly HttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _logger;
        private readonly AuthorizeCommandHandler _handler;

        public AuthorizeCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _paymentMSAuthorizeCommand = new PaymentMSAuthorizeCommandHandler(_httpConnector);
            _airportDetailsQuery = new AirportDetailsQueryHandler(_httpConnector);
            _currencyDetailsQuery = new CurrencyDetailsQueryHandler(_httpConnector);
            _handler = new AuthorizeCommandHandler(_paymentMSAuthorizeCommand, _airportDetailsQuery, _currencyDetailsQuery);
        }

        [Fact]
        public async Task AuthorizeSuccess()
        {
            ConfigReader.Set("PaymentMSSubscriptionKey", "dummyKey123456");
            ConfigReader.Set("PaymentMSUrl", "https://testresources.virginatlantic.com/dev/orderpaymentmicroservice/");
            ConfigReader.Set("ReferenceMSSubscriptionKey", "dummyKey123456");
            ConfigReader.Set("ReferenceMSUrl", "https://testresources.virginatlantic.com/dev/referencemicroservice/");

            var authorizeRequest = TestDataProvider.GetTestData<Domain.AuthorizeRequest>("NdcIfgFacade\\AuthorizeRequest");

            var result = await _handler.Handle(authorizeRequest, CancellationToken.None);
            
            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Value.Value.TransactionId);
            Assert.Equal("AUTH_ACCEPTED", result.Value.Value.PaymentStatus);
        }

        [Fact]
        public async Task AuthorizeRequestMapper()
        {
            ConfigReader.Set("ReferenceMSSubscriptionKey", "dummyKey123456");
            ConfigReader.Set("ReferenceMSUrl", "https://testresources.virginatlantic.com/dev/referencemicroservice/");

            var authorizeRequest = TestDataProvider.GetTestData<AuthorizeRequest>("NdcIfgFacade\\AuthorizeRequest");

            var airportDetails = await _airportDetailsQuery.Handle();
            var currencyDetails = await _currencyDetailsQuery.Handle();

            var authorizePaymentMSRequest = PaymentRequestMapper.MapAuthorizeRequest(authorizeRequest, airportDetails, currencyDetails);

            Assert.NotNull(authorizePaymentMSRequest);
            Assert.Equal("2026-06-10T10:55", authorizePaymentMSRequest.Segments.First().ScheduledDepartureTime.LocalDateTime);
            var participant = authorizePaymentMSRequest.Parties.Participants.First();
            Assert.Equal("5831047", participant.AgencyID);
            Assert.Equal("Northstar Travel Group", participant.Name);
            Assert.Equal("northstar.agent5831047@example.test", participant.Contacts.Email);
            Assert.Equal(Participant.PartyTypeEnum.Seller, participant.PartyType);
            Assert.Equal("IATA", participant.RequestAgentType);
        }

        [Fact]
        public async Task AuthorizeRequestMapper_LeavesTravelAgentDetailsUnset_WhenMerchantInformationIsMissing()
        {
            ConfigReader.Set("ReferenceMSSubscriptionKey", "dummyKey123456");
            ConfigReader.Set("ReferenceMSUrl", "https://testresources.virginatlantic.com/dev/referencemicroservice/");

            var authorizeRequest = TestDataProvider.GetTestData<AuthorizeRequest>("NdcIfgFacade\\AuthorizeRequest");
            authorizeRequest.FormOfPayment.First().MerchantDefinedInformation = null;

            var airportDetails = await _airportDetailsQuery.Handle();
            var currencyDetails = await _currencyDetailsQuery.Handle();

            var authorizePaymentMSRequest = PaymentRequestMapper.MapAuthorizeRequest(authorizeRequest, airportDetails, currencyDetails);
            var participant = authorizePaymentMSRequest.Parties.Participants.First();

            Assert.Null(participant.Name);
            Assert.Null(participant.Contacts.Email);
        }

        [Fact]
        public async Task CaptureRequestMapper_MapsTravelAgencyFields()
        {
            ConfigReader.Set("ReferenceMSSubscriptionKey", "dummyKey123456");
            ConfigReader.Set("ReferenceMSUrl", "https://testresources.virginatlantic.com/dev/referencemicroservice/");

            var captureRequest = TestDataProvider.GetTestData<Domain.OrderRequest>("NdcIfgFacade\\CaptureRequest");
            var airportDetails = await _airportDetailsQuery.Handle();
            var currencyDetails = await _currencyDetailsQuery.Handle();

            var capturePaymentMSRequest = PaymentRequestMapper.MapCaptureRequest(captureRequest, airportDetails, currencyDetails, null);

            var participant = capturePaymentMSRequest.Parties.Participants.First();
            Assert.Equal("5831047", participant.AgencyID);
            Assert.Equal("Northstar Travel Group", participant.Name);
            Assert.Equal("northstar.agent5831047@example.test", participant.Contacts.Email);
        }

        [Fact]
        public async Task AuthorizeValidation_Failure()
        {
            var authorizeRequest = new AuthorizeRequest()
            {
                Orders = new List<Order>()
                {
                    new Order()
                    {
                        FlightRecords =
                        [
                            new FlightRecord()
                            {
                                PassengerRecord = new PassengerRecord()
                                {
                                    Pnr = "PNR123"
                                }
                            }
                        ]
                    }
                },
                FormOfPayment =
                [
                    new FormOfPayment()
                    {
                        Amount = 1000,
                        Currency = "USD"
                    }
                ]
            };

            var result = await _handler.Handle(authorizeRequest, CancellationToken.None);

            Assert.NotNull(result.Value.Errors);
            Assert.Equal("Request Orders FlightRecords DocumentDetails cannot be null.", result.Value.Errors.First().Message);

        }

        [Fact]
        public async Task AuthorizeHandler_ReturnsControlledError_WhenDependencyFails()
        {
            var request = TestDataProvider.GetTestData<AuthorizeRequest>("NdcIfgFacade\\AuthorizeRequest");
            var logger = new RecordingLogger<AuthorizeCommandHandler>();
            var handler = new AuthorizeCommandHandler(
                null,
                new ThrowingAirportDetailsQuery(new InvalidOperationException("synthetic failure")),
                null,
                logger);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.Equal(Status.Error, result.Value.Status);
            Assert.Equal("Payment authorization failed.", result.Value.Errors.First().Message);
            Assert.Equal(LogLevel.Error, logger.LastLevel);
        }

        [Fact]
        public void ParsePhone_Test() 
        {
            //Arrange
            var phoneNumber = "1-5551234567";
            var ukPhoneNumber = "44-1234567890";
            var leadingZeroNumber = "0044-1239876540";
            
            //Act
            var (countryCode, phone) = PaymentRequestMapper.ParsePhone(phoneNumber);
            var (ukCountryCode, ukPhone) = PaymentRequestMapper.ParsePhone(ukPhoneNumber);
            var (leadingZeroCountryCode, leadingZeroPhone) = PaymentRequestMapper.ParsePhone(leadingZeroNumber);

            //Assert
            Assert.Equal("1", countryCode);
            Assert.Equal("5551234567", phone);
            Assert.Equal("44", ukCountryCode);
            Assert.Equal("1234567890", ukPhone);
            Assert.Equal("44", leadingZeroCountryCode);

        }
    }
}



