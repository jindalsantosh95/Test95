using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Application;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain.IATAAcknowledgement;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Mapper;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Tests
{
    public class OrderChangeNotifCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly HttpConnector _httpConnector;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAgentOCNEndpointService _agentOCNEndpointService;
        private readonly IPortalOCNEndpointService _portalOCNEndpointService;

        public OrderChangeNotifCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _serviceProvider = new DefaultServiceProvider();
            _mediator = new Mediator(_serviceProvider);
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _agentOCNEndpointService = new AgentOCNEndpointsService(_httpConnector, Microsoft.Extensions.Logging.Abstractions.NullLogger<AgentOCNEndpointsService>.Instance);
            _portalOCNEndpointService = new PortalOCNEndpointService(_httpConnector);
            ConfigReader.Set("OCNPortalEndpoint", "https://test/ocn/portal/");
            ConfigReader.Set("OCNPortalClientKey", "testKey");
            ConfigReader.Set("OCNPortalClientSecret", "test");
            ConfigReader.Set("OCNEnvironment", "TEST");
            ConfigReader.Set("OCNCredentialsDecrytionKey", "123");
        }

        [Fact]
        public async Task ShouldReturnSuccessResult_WhenValidRequestIsPassed()
        {
            //Arrange
            var commandHandler = new OrderChangeNotifCommandHandler(_agentOCNEndpointService, _portalOCNEndpointService, Microsoft.Extensions.Logging.Abstractions.NullLogger<OrderChangeNotifCommandHandler>.Instance);
            var body = TestDataProvider.GetTestData<OrderChangeNotifRequest>("NDCOrderNotificationMS\\OrderNotificationRequest");

            var request = new OrderChangeNotifRequest()
            {
                OrderDocument = body.OrderDocument,
                AgentEndpointDetails = body.AgentEndpointDetails
            };

            //Act
            var result = await commandHandler.Handle(request);

            //Assert
            Assert.True(result.IsSuccess);
            var response = result.Value.Value;

            Assert.Equal(2, response.AgentResponse.Count());
            Assert.Contains(response.AgentResponse, x => x.AgentId == "123" && x.AcknowledgementRcvd);
            Assert.Contains(response.AgentResponse, x => x.AgentId == "portal" && x.AcknowledgementRcvd);
        }

        [Fact]
        public async Task ShouldAggregateDuplicateAgencyIdsCaseInsensitively_WhenEndpointAcknowledgementsDiffer()
        {
            // Arrange
            var body = TestDataProvider.GetTestData<OrderChangeNotifRequest>("NDCOrderNotificationMS\\OrderNotificationRequest");
            var firstEndpoint = body.AgentEndpointDetails.First();
            var request = new OrderChangeNotifRequest
            {
                OrderDocument = body.OrderDocument,
                AgentEndpointDetails = new[]
                {
                    new AgencyOCNDetails
                    {
                        AgencyDetails = new AgentDetails { RequestAgencyId = "agent59999" },
                        EndpointDetails = firstEndpoint.EndpointDetails
                    },
                    new AgencyOCNDetails
                    {
                        AgencyDetails = new AgentDetails { RequestAgencyId = "AGENT59999" },
                        EndpointDetails = firstEndpoint.EndpointDetails
                    }
                }
            };
            var handler = new OrderChangeNotifCommandHandler(
                new MixedAcknowledgementAgentService(),
                new SuccessfulPortalService(),
                Microsoft.Extensions.Logging.Abstractions.NullLogger<OrderChangeNotifCommandHandler>.Instance);

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.True(result.IsSuccess);
            var responses = result.Value.Value.AgentResponse.ToList();
            Assert.Equal(2, responses.Count);
            var agencyResponse = Assert.Single(responses, response => response.AgentId == "agent59999");
            Assert.False(agencyResponse.AcknowledgementRcvd);
            Assert.Single(responses, response => response.AgentId == "portal");
        }

        [Fact]
        public async Task ShouldProcessRegisteredPortalAgencyEndpoint_WhenItAppearsBeforeAnotherAgency()
        {
            // Arrange
            var body = TestDataProvider.GetTestData<OrderChangeNotifRequest>("NDCOrderNotificationMS\\OrderNotificationRequest");
            var endpointDetails = body.AgentEndpointDetails.First().EndpointDetails;
            var request = new OrderChangeNotifRequest
            {
                OrderDocument = body.OrderDocument,
                AgentEndpointDetails = new[]
                {
                    new AgencyOCNDetails
                    {
                        AgencyDetails = new AgentDetails { RequestAgencyId = "portal" },
                        EndpointDetails = endpointDetails
                    },
                    new AgencyOCNDetails
                    {
                        AgencyDetails = new AgentDetails { RequestAgencyId = "agency59999" },
                        EndpointDetails = endpointDetails
                    }
                }
            };
            var handler = new OrderChangeNotifCommandHandler(
                new MixedAcknowledgementAgentService(),
                new SuccessfulPortalService(),
                Microsoft.Extensions.Logging.Abstractions.NullLogger<OrderChangeNotifCommandHandler>.Instance);

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.True(result.IsSuccess);
            var responses = result.Value.Value.AgentResponse.ToList();
            Assert.Equal(3, responses.Count);
            Assert.Equal(2, responses.Count(response => response.AgentId == "portal"));
            Assert.Contains(responses, response => response.AgentId == "agency59999" && !response.AcknowledgementRcvd);
        }

        private sealed class MixedAcknowledgementAgentService : IAgentOCNEndpointService
        {
            private int callCount;

            public Task<IATAAcknowledgement> Send(IATAOrderChangeNotifRQ request, AgentEndpointDetails agentEndpointDetails)
            {
                callCount++;
                return Task.FromResult(callCount == 1
                    ? new IATAAcknowledgement { Notification = new AckType { StatusCode = "OK" } }
                    : null);
            }
        }

        private sealed class SuccessfulPortalService : IPortalOCNEndpointService
        {
            public Task<IATAAcknowledgement> Send(IATAOrderChangeNotifRQ request) =>
                Task.FromResult(new IATAAcknowledgement { Notification = new AckType { StatusCode = "OK" } });
        }
    

        [Fact]
        public void MapRequest_WhenLatestHistoryIsDocumentIssuance_ShouldSetExpectedCodes_AndSuppressOldTicketDocInfo()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<OrderChangeNotifRequest>("NDCOrderNotificationMS\\OrderNotificationRequest");
            var orderDocument = request.OrderDocument;

            var latestHistory = orderDocument.OrderHistory.OrderBy(x => x.Version).Last();
            latestHistory.ChangeType = ChangeTypeEnum.DocumentIssuance;

            // Act
            var result = OrderChangeNotifRQMapper.MapRequest(orderDocument);
            var changeOperation = result.Notification.ChangeGroup.First().ChangeOperation.First();

            // Assert
            Assert.Equal("112", changeOperation.ReasonCode);
            Assert.Equal("10", changeOperation.ChangeTypeCode);
            Assert.Null(changeOperation.Old.TicketDocInfo);
        }

        [Fact]
        public void MapRequest_WhenLatestHistoryIsDocumentExchanged_ShouldSetExpectedCodes_AndSuppressOldTicketDocInfo()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<OrderChangeNotifRequest>("NDCOrderNotificationMS\\OrderNotificationRequest");
            var orderDocument = request.OrderDocument;

            var latestHistory = orderDocument.OrderHistory.OrderBy(x => Convert.ToInt32(x.Version)).Last();
            latestHistory.ChangeType = ChangeTypeEnum.DocumentExchanged;

            // Act
            var result = OrderChangeNotifRQMapper.MapRequest(orderDocument);
            var changeOperation = result.Notification.ChangeGroup.First().ChangeOperation.First();

            // Assert
            Assert.Equal("112", changeOperation.ReasonCode);
            Assert.Equal("16", changeOperation.ChangeTypeCode);
            Assert.Null(changeOperation.Old.TicketDocInfo);

            var oldOrder = Assert.Single(changeOperation.Old.Order);
            Assert.Equal(latestHistory.Version, oldOrder.OrderVersionNumber);
            Assert.True(oldOrder.LastModifiedDateTimeSpecified);
            Assert.Equal(DateTimeOffset.Parse(latestHistory.ChangeDateTime, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal).UtcDateTime, oldOrder.LastModifiedDateTime);
        }
    }
}
