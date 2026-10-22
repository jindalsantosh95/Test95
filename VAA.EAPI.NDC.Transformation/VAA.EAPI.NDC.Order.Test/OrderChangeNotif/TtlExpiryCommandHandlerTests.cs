using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChangeNotif;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderNotificationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using Xunit;

namespace VAA.EAPI.NDC.Order.Tests.OrderChangeNotif
{
    public class TtlExpiryCommandHandlerTests
    {
        private const string OrderDataUrl = "https://test/orderdata/orders/";
        private const string OrderSearchUrl = "https://test/ordersearch";
        private const string ValidationUrl = "https://test/validation";
        private const string AgencyOcnDetailsUrl = "https://test/validation/AgencyOCNDetails/";

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle_UsesExpectedAgencyOcnDetailsRequest_ForDistributorPresence(bool includeDistributor)
        {
            var orderDocumentJson = ReadOrderDocument(includeDistributor);
            var reservationOrderDocumentJson = ReadReservationOrderDocument(orderDocumentJson);
            var agencyOcnRequestUris = new List<string>();
            var notification = new Mock<IOrderNotificationMSCommand>();
            notification.Setup(command => command.Handle(It.IsAny<VAA.EAPI.NDC.Order.Infrastructure.Model.OrderNotificationMS.OrderNotificationRequest>()))
                .ReturnsAsync(new VAA.EAPI.NDC.Order.Infrastructure.Model.OrderNotificationMS.OrderNotificationResponse());

            var httpConnector = new Mock<IHttpConnector>();
            httpConnector.Setup(connector => connector.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<HttpMethod>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns((string uri, Dictionary<string, string> _, string _, HttpMethod _, string _, bool _) =>
                {
                    if (uri.StartsWith(OrderDataUrl, StringComparison.OrdinalIgnoreCase))
                        return Task.FromResult(Ok(orderDocumentJson));

                    if (uri.Equals(OrderSearchUrl, StringComparison.OrdinalIgnoreCase))
                        return Task.FromResult(Ok(reservationOrderDocumentJson));

                    if (uri.Equals(ValidationUrl, StringComparison.OrdinalIgnoreCase))
                        return Task.FromResult(Ok("{\"OCNFlag\":true}"));

                    if (uri.StartsWith(AgencyOcnDetailsUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        agencyOcnRequestUris.Add(uri);
                        return Task.FromResult(Ok("[{\"AgencyDetails\":{\"RequestAgencyId\":\"00003333\"},\"EndpointDetails\":{\"Endpoint\":\"https://agent/ocn\",\"EndpointId\":1}}]"));
                    }

                    return Task.FromResult(Ok(string.Empty));
                });

            ConfigureSettings();
            var analytics = new Mock<ITrackAnalyticsEventCommandHandler>();
            analytics.Setup(command => command.Handle(It.IsAny<TrackAnalyticsEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new TtlExpiryCommandHandler(
                NullLogger<TtlExpiryCommandHandler>.Instance,
                new PublishOrderDocMSCommandHandler(httpConnector.Object),
                new OrderDataMSQueryHandler(httpConnector.Object),
                new ValidationMSAgencyOCNDetailsQueryHandler(httpConnector.Object),
                new OrderSearchMSCommandHandler(httpConnector.Object),
                new ValidationMSCommandHandler(httpConnector.Object),
                notification.Object,
                new Mock<IBrandMSCommand>().Object,
                new Mock<IBrandMSPTCCommand>().Object,
                analytics.Object);

            await handler.Handle(new OcnTtlExpiryRQ
            {
                BookingReference = new BookingReference { RLOC = "SIVOZP" }
            }, "TTLExpiry", CancellationToken.None);

            var expectedUri = AgencyOcnDetailsUrl + "00003333" + (includeDistributor ? "?aggregatorId=AGGREGATOR1" : string.Empty);
            Assert.Equal(new[] { expectedUri }, agencyOcnRequestUris);
            notification.Verify(command => command.Handle(It.IsAny<VAA.EAPI.NDC.Order.Infrastructure.Model.OrderNotificationMS.OrderNotificationRequest>()), Times.Once);
        }

        private static void ConfigureSettings()
        {
            ConfigReader.Set(Constants.Environment.OrderDataMSEndpoint, OrderDataUrl);
            ConfigReader.Set(Constants.Environment.OrderDataMSSubscriptionKey, "test");
            ConfigReader.Set(Constants.Environment.OrderSearchMSUrl, OrderSearchUrl);
            ConfigReader.Set(Constants.Environment.OrderSearchMSSubscriptionKey, "test");
            ConfigReader.Set(Constants.Environment.ValidationMSUrl, ValidationUrl);
            ConfigReader.Set(Constants.Environment.ValidationMSSubscriptionKey, "test");
            ConfigReader.Set(Constants.Environment.ValidationMSAgencyOCNDetailsUrl, AgencyOcnDetailsUrl);
            ConfigReader.Set(Constants.Environment.AgentRegistrationEnvironment, "test");
            ConfigReader.Set(Constants.Environment.PublishOrderDocUrl, "https://test/publish");
        }

        private static string ReadAggregatorOrderDocument()
        {
            var mock = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, ".mocks", "OrderDataMSQueryHandlerOCNTimeChangeOrderDocWithAggregator.mock"));
            return mock[(mock.IndexOf("\n{", StringComparison.Ordinal) + 1)..];
        }

        private static string ReadOrderDocument(bool includeDistributor)
        {
            var orderDocument = Json.Deserialize<OrderDocument>(ReadAggregatorOrderDocument());

            if (!includeDistributor)
            {
                orderDocument.Parties.Participants = orderDocument.Parties.Participants
                    .Where(participant => participant.PartyType != VAA.EAPI.NDC.Order.Infrastructure.Model.Participant.PartyTypeEnum.Distributor)
                    .ToList();
            }

            return Json.Serialize(orderDocument);
        }

        private static string ReadReservationOrderDocument(string orderDocumentJson)
        {
            var orderDocument = Json.Deserialize<OrderDocument>(orderDocumentJson);

            foreach (var orderItem in orderDocument.OrderItems)
                orderItem.AccountableDocumentRefs = null;

            return Json.Serialize(orderDocument);
        }

        private static HttpResponseMessage Ok(string body) =>
            new(HttpStatusCode.OK) { Content = new StringContent(body) };
    }
}
