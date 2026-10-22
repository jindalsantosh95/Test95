using System.Threading;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.DesignatePrinter;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TKTExchange;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using CreateSessionSecurity = VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession.Security;
using CreateSessionSecurityBinarySecurityToken = VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession.SecurityBinarySecurityToken;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class OrderChangeAcceptDisruptionCommandHandlerTests
    {
        public OrderChangeAcceptDisruptionCommandHandlerTests()
        {
            VAA.EAPI.Common.Helpers.ConfigReader.Set("ConversationId", "test-conversation-id");
            VAA.EAPI.Common.Helpers.ConfigReader.Set("FromPartyId", "test-from-party");
            VAA.EAPI.Common.Helpers.ConfigReader.Set("ToPartyId", "test-to-party");
            VAA.EAPI.Common.Helpers.ConfigReader.Set("DirectTicketingPCC", "6UOL");
            VAA.EAPI.Common.Helpers.ConfigReader.Set("KeyVaultUri", "https://tempuri.org");
            VAA.EAPI.Common.Helpers.ConfigReader.Set("Domain", "test-domain");
        }

        [Fact]
        public async Task CancellationAccept_WithUnticketedReservation_SkipsTicketExchangeAndElectronicDocumentLookup()
        {
            // Arrange
            var ticketExchange = new TrackingTicketExchangeCommand();
            var electronicDocument = new TrackingElectronicDocument();
            var handler = CreateCancellationHandler(ticketExchange, electronicDocument);
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt\\DisruptedOrderChangeReqTimeChange");

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.Empty(result.Value.Value.OrderStatus);
            Assert.Equal(0, ticketExchange.CallCount);
            Assert.Equal(0, electronicDocument.CallCount);
        }

        [Fact]
        public async Task TimeChangeAccept_WithUnticketedReservation_SkipsElectronicDocumentLookup()
        {
            // Arrange
            var electronicDocument = new TrackingElectronicDocument();
            var handler = CreateTimeChangeHandler(electronicDocument);
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt\\DisruptedOrderChangeReqTimeChange");

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.Empty(result.Value.Value.OrderStatus);
            Assert.Equal(0, electronicDocument.CallCount);
        }

        [Fact]
        public async Task CancellationAccept_WithTicketedReservation_ExchangesTicketsAndLooksUpElectronicDocuments()
        {
            // Arrange
            var ticketExchange = new TrackingTicketExchangeCommand();
            var electronicDocument = new TrackingElectronicDocument();
            var handler = CreateCancellationHandler(new TicketedGetReservation(), ticketExchange, electronicDocument);
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt\\DisruptedOrderChangeReqTimeChange");

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Confirmed", result.Value.Value.OrderStatus);
            Assert.True(ticketExchange.CallCount > 0);
            Assert.True(electronicDocument.CallCount > 0);
        }

        [Fact]
        public async Task TimeChangeAccept_WithTicketedReservation_LooksUpElectronicDocuments()
        {
            // Arrange
            var electronicDocument = new TrackingElectronicDocument();
            var handler = CreateTimeChangeHandler(new TicketedGetReservation(), electronicDocument);
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt\\DisruptedOrderChangeReqTimeChange");

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Confirmed", result.Value.Value.OrderStatus);
            Assert.True(electronicDocument.CallCount > 0);
        }

        private static OrderChangeAcceptCancellationDisruptionCommandHandler CreateCancellationHandler(
            TrackingTicketExchangeCommand ticketExchange,
            TrackingElectronicDocument electronicDocument)
        {
            return CreateCancellationHandler(new UnticketedGetReservation(), ticketExchange, electronicDocument);
        }

        private static OrderChangeAcceptCancellationDisruptionCommandHandler CreateCancellationHandler(
            IGetReservation getReservationService,
            TrackingTicketExchangeCommand ticketExchange,
            TrackingElectronicDocument electronicDocument)
        {
            var getReservation = new GetReservationQueryHandler(getReservationService);

            return new OrderChangeAcceptCancellationDisruptionCommandHandler(
                getReservation,
                new CreateSessionCommandHandler(new ApprovedSession()),
                new EnhancedEndTransactionCommandHandler(new TestEnhancedEndTransaction()),
                new GetElectronicDocumentQueryHandler(electronicDocument),
                new SessionCloseCommandHandler(new TestSessionClose()),
                new DesignatePrinterCommandHandler(new SuccessfulDesignatePrinter()),
                ticketExchange);
        }

        private static OrderChangeAcceptTimeChangeDisruptionCommandHandler CreateTimeChangeHandler(
            TrackingElectronicDocument electronicDocument)
        {
            return CreateTimeChangeHandler(new UnticketedGetReservation(), electronicDocument);
        }

        private static OrderChangeAcceptTimeChangeDisruptionCommandHandler CreateTimeChangeHandler(
            IGetReservation getReservationService,
            TrackingElectronicDocument electronicDocument)
        {
            var getReservation = new GetReservationQueryHandler(getReservationService);

            return new OrderChangeAcceptTimeChangeDisruptionCommandHandler(
                getReservation,
                new CreateSessionCommandHandler(new ApprovedSession()),
                new EnhancedEndTransactionCommandHandler(new TestEnhancedEndTransaction()),
                new GetElectronicDocumentQueryHandler(electronicDocument),
                new SessionCloseCommandHandler(new TestSessionClose()));
        }

        private sealed class UnticketedGetReservation : IGetReservation
        {
            public Task<GetReservationRS> GetReservationAsync(GetReservationOperationRequest request)
            {
                var response = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreOrderChangeMgt\\DisruptedGetReservRSTimeChange");
                ((ReservationPNRB)response.Item).PassengerReservation.TicketingInfo = null;
                return Task.FromResult(response);
            }
        }

        private sealed class TicketedGetReservation : IGetReservation
        {
            public Task<GetReservationRS> GetReservationAsync(GetReservationOperationRequest request)
            {
                return Task.FromResult(TestDataProvider.GetXmlTestData<GetReservationRS>("SabreOrderChangeMgt\\DisruptedGetReservRSTimeChange"));
            }
        }

        private sealed class ApprovedSession : ICreateSession
        {
            public Task<SessionCreateRQResponse> CreateSessionAsync(SessionCreateRQRequest request)
            {
                return Task.FromResult(new SessionCreateRQResponse
                {
                    SessionCreateRS = new SessionCreateRS { status = "Approved" },
                    Security = new CreateSessionSecurity
                    {
                        BinarySecurityToken = new CreateSessionSecurityBinarySecurityToken { Value = "test-token" }
                    }
                });
            }
        }

        private sealed class SuccessfulDesignatePrinter : IDesignatePrinter
        {
            public Task<DesignatePrinterRQResponse> DesignatePrinterAsync(DesignatePrinterRQRequest request)
            {
                return Task.FromResult(new DesignatePrinterRQResponse());
            }
        }

        private sealed class TrackingTicketExchangeCommand : ITKTExchangeCommand
        {
            public int CallCount { get; private set; }

            public Task<ExchangeRS> Exchange(ExchangeRQRequest request)
            {
                CallCount++;
                return Task.FromResult(new ExchangeRS());
            }
        }

        private sealed class TrackingElectronicDocument : IGetElectronicDocument
        {
            public int CallCount { get; private set; }

            public Task<GetElectronicDocumentRS> GetElectronicDocumentRQAsync(GetElectronicDocumentRQRequest request)
            {
                CallCount++;
                return Task.FromResult(new GetElectronicDocumentRS());
            }
        }
    }
}
