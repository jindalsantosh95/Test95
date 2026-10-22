using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Email.Client;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.Email.Application.Command;
using VAA.EAPI.Microservices.CRM.Email.Domain;
using VAA.EAPI.Microservices.CRM.Email.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.CRM.Email.Tests
{
    public class SendOrderEmailCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly HttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _logger;
        private readonly ILogger<ACMClient> _acmlogger;
        private readonly ILogger<SendOrderEmailCommandHandler> _handlerLogger;
        private readonly IACMClient _aCMClient;
        private readonly ITokenHelper _tokenHelper;
        private readonly SendOrderEmailCommandHandler _handler;

        public SendOrderEmailCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _acmlogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ACMClient>.Instance;
            _tokenHelper = new TokenHelper(_httpConnector);
            _aCMClient = new ACMClient(_acmlogger, _httpConnector, _tokenHelper);
            _handlerLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<SendOrderEmailCommandHandler>.Instance;
            _handler = new SendOrderEmailCommandHandler(_aCMClient, _handlerLogger);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenACMClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SendOrderEmailCommandHandler(null, _handlerLogger));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SendOrderEmailCommandHandler(_aCMClient, null));
        }

        [Fact]
        public async Task SendOrderEmail_ShouldSucceed()
        {
            //Arrange
            var orderDocument = TestDataProvider.GetTestData<Domain.OrderDocument>("EmailMS\\OrderDocument");
            
            ConfigReader.Set("enableemailmsauthentication", "false");
            ConfigReader.Set("acmsessiontoken", "test_token");
            ConfigReader.Set("bookingacknowledgmenttemplate", "BKGACK");
            ConfigReader.Set("acmcommunicationchannel", "0");
            ConfigReader.Set("enableadobenewendpoint", "true");
            ConfigReader.Set("soapendpointuriv2", "https://virginatlantic-rt-stage4-all-lb.campaign.adobe.com/nl/jsp/soaprouter.jsp");
            
            var request = new SendOrderEmailRequest()
            {
                RequestType = Constants.RequestType.OrderCreate,
                OrderDocument = orderDocument
            };

            //Act
            await _handler.Handle(request, CancellationToken.None);

        }

        [Fact]
        public void OrderEmailToACMRequestMapper_Test()
        {
            //Arrange
            var orderDocument = TestDataProvider.GetTestData<Domain.OrderDocument>("EmailMS\\OrderDocument");

            ConfigReader.Set("enableemailmsauthentication", "false");
            ConfigReader.Set("acmsessiontoken", "test_token");
            ConfigReader.Set("bookingacknowledgmenttemplate", "BKGACK");
            ConfigReader.Set("acmcommunicationchannel", "0");
            
            var request = new SendOrderEmailRequest()
            {
                RequestType = Constants.RequestType.OrderCreate,
                OrderDocument = orderDocument
            };

            //Act
            var response = OrderEmailToACMRequestMapper.MapRequest(request);


            var doc = XDocument.Parse(response);

            var email = doc.Descendants("rtEvent").Single().Attribute("email")?.Value;
            var rtEvent = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "rtEvent");
            var typeAttribute = rtEvent?.Attribute("type")?.Value;

            // Assert
            Assert.NotEmpty(response);
            Assert.NotNull(email);
            Assert.Equal("agentemail@gmail.com", email);

            Assert.NotNull(rtEvent);
            Assert.Equal("BKGACK", typeAttribute);

        }

        [Fact]
        public void OrderEmailToACMHoldBookingRQMapper_Test()
        {
            var orderDocument = TestDataProvider.GetTestData<Domain.OrderDocument>("EmailMS\\OrderDocument");

            ConfigReader.Set("enableemailmsauthentication", "false");
            ConfigReader.Set("acmsessiontoken", "test_token");
            ConfigReader.Set("holdbookingtemplate", "HBFACK");
            ConfigReader.Set("acmcommunicationchannel", "0");

            var request = new SendOrderEmailRequest()
            {
                RequestType = Constants.RequestType.HoldBooking,
                OrderDocument = orderDocument
            };

            var response = OrderEmailToACMHoldBookingRQMapper.MapRequest(request);

            var doc = XDocument.Parse(response);

            var email = doc.Descendants("rtEvent").Single().Attribute("email")?.Value;
            var rtEvent = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "rtEvent");
            var typeAttribute = rtEvent?.Attribute("type")?.Value;

            Assert.NotEmpty(response);
            Assert.NotNull(email);
            Assert.Equal("agentemail@gmail.com", email);
            Assert.Contains("HBFACK", response);
        }
    }
}
