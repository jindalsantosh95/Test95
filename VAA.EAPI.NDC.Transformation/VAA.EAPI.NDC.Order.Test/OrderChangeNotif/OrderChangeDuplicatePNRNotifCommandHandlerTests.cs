using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChangeNotif;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderNotificationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.NdcQueueMgmt;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderNotificationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using Xunit;

namespace VAA.EAPI.NDC.Order.Tests.OrderChangeNotif
{
    public class OrderChangeDuplicatePNRNotifCommandHandlerTests
    {
        private const string MainPnr = "PLRXW1";
        private const string OtherPnr = "PLQHCB";
        private const string MainPnr2 = "ZXY123";
        private const string OtherPnr2 = "ABCDEF";

        [Fact]
        public async Task DuplicatePnr_AllAgentIdsSame_SendsOcn_AndRemovesMainQueueItem()
        {
            var analytics = new FakeTrackAnalyticsEventCommandHandler();
            var orderNotification = new FakeOrderNotificationMSCommand(new OrderNotificationResponse
            {
                OrderNotificationAgentResponse = new OrderNotificationAgentResponse
                {
                        AgentResponse = new[]
                        {
                            new AgentResponse
                            {
                                AgentId = "AG1",
                                Status = VAA.EAPI.Domain.Entities.Common.Status.Accepted,
                                AcknowledgementRcvd = true
                            },
                            new AgentResponse
                            {
                                AgentId = "AG2",
                                Status = VAA.EAPI.Domain.Entities.Common.Status.Accepted,
                                AcknowledgementRcvd = true
                            }
                    }
                }
            });
            var fetch = new FakeNdcQueueMgmtFetchDuplicatePnrsCommand(new List<string> { MainPnr });
            var remove = new FakeNdcQueueMgmtRemoveDuplicatePnrsCommand();
            var logger = new RecordingLogger<OrderChangeDuplicatePNRNotifCommandHandler>();

            var httpConnector = new StubHttpConnector((uri, method, body) =>
            {
                if (uri.StartsWith(OrderDataEndpoint, StringComparison.OrdinalIgnoreCase))
                {
                    var pnr = uri.Substring(OrderDataEndpoint.Length);
                    return Ok(JsonOrderDataOrderDoc(pnr));
                }

                if (uri.Is(OrderSearchUrl))
                {
                    var isMain = body != null && body.Contains(MainPnr, StringComparison.OrdinalIgnoreCase);
                    var pnr = isMain ? MainPnr : OtherPnr;
                    return Ok(JsonGetReservationDoc(pnr, sellerAgencyId: "AG1", includeDuplicateRemark: isMain));
                }

                if (uri.Is(ValidationUrl))
                    return Ok("{\"OCNFlag\":true}");

                if (uri.StartsWith(AgencyOcnDetailsUrl, StringComparison.OrdinalIgnoreCase))
                    return Ok("[{\"AgencyDetails\":{\"RequestAgencyId\":\"AG1\"},\"EndpointDetails\":{\"Endpoint\":\"https://agent/ocn\",\"EndpointId\":1}}]");

                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
            });

            ConfigureCommonSettings();

            var handler = BuildHandler(httpConnector, analytics, orderNotification, fetch, remove, logger);

            // Act
            var result = await handler.Handle(new OrderChangeDuplicatePNRNotifRQ { UniqueId = MainPnr, PCC = "ABCD" }, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            var notificationRequest = Assert.Single(orderNotification.Requests);
            Assert.True(notificationRequest.IsDuplicatePNR);
            Assert.Equal("DuplicatePNR", notificationRequest.TTLExpiryType);
            Assert.NotNull(notificationRequest.OrderDocument?.BookingReference);
            Assert.Equal(MainPnr, notificationRequest.OrderDocument.BookingReference.RLOC);
            Assert.Equal(new[] { MainPnr }, remove.RemovedPnrs);
            Assert.Contains(logger.Messages, message => message == "DuplicatePNR OCN sent. PNR=PLRXW1");
            var analyticsRequest = Assert.Single(analytics.Requests);
            Assert.Equal(MainPnr, analyticsRequest.GetValue("PNR"));
            Assert.Equal("Acknowledged", analyticsRequest.GetValue("OcnStatus"));
            Assert.Equal(2, Convert.ToInt32(analyticsRequest.GetValue("AgentAcknowledgementCount")));
            Assert.Equal("Removed", analyticsRequest.GetValue("QueueRemovalStatus"));
        }

        [Fact]
        public async Task DuplicatePnr_OrderNotificationThrows_LogsFailureWithException()
        {
            var analytics = new FakeTrackAnalyticsEventCommandHandler();
            var notificationException = new Exception("OrderNotificationMS token=secret-token");
            var orderNotification = new FakeOrderNotificationMSCommand(
                new OrderNotificationResponse(),
                exception: notificationException);
            var fetch = new FakeNdcQueueMgmtFetchDuplicatePnrsCommand(new List<string> { MainPnr });
            var remove = new FakeNdcQueueMgmtRemoveDuplicatePnrsCommand();
            var logger = new RecordingLogger<OrderChangeDuplicatePNRNotifCommandHandler>();

            var httpConnector = new StubHttpConnector((uri, method, body) =>
            {
                if (uri.StartsWith(OrderDataEndpoint, StringComparison.OrdinalIgnoreCase))
                {
                    var pnr = uri.Substring(OrderDataEndpoint.Length);
                    return Ok(JsonOrderDataOrderDoc(pnr));
                }

                if (uri.Is(OrderSearchUrl))
                {
                    var isMain = body != null && body.Contains(MainPnr, StringComparison.OrdinalIgnoreCase);
                    var pnr = isMain ? MainPnr : OtherPnr;
                    return Ok(JsonGetReservationDoc(pnr, sellerAgencyId: "AG1", includeDuplicateRemark: isMain));
                }

                if (uri.Is(ValidationUrl))
                    return Ok("{\"OCNFlag\":true}");

                if (uri.StartsWith(AgencyOcnDetailsUrl, StringComparison.OrdinalIgnoreCase))
                    return Ok("[{\"AgencyDetails\":{\"RequestAgencyId\":\"AG1\"},\"EndpointDetails\":{\"Endpoint\":\"https://agent/ocn\",\"EndpointId\":1}}]");

                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
            });

            ConfigureCommonSettings();

            var handler = BuildHandler(httpConnector, analytics, orderNotification, fetch, remove, logger);

            var result = await handler.Handle(new OrderChangeDuplicatePNRNotifRQ { UniqueId = MainPnr, PCC = "ABCD" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { MainPnr }, remove.RemovedPnrs);
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR OCN item processing failed. PNR=PLRXW1");
            Assert.Contains(logger.Exceptions, exception => ReferenceEquals(notificationException, exception));
            var analyticsRequest = Assert.Single(analytics.Requests);
            Assert.Equal("Exception", analyticsRequest.GetValue("ExceptionType"));
            Assert.Equal("OcnItemProcessingFailed", analyticsRequest.GetValue("FailureReason"));
            Assert.Throws<ArgumentOutOfRangeException>(() => analyticsRequest.GetValue("ExceptionMessage"));
        }

        [Fact]
        public async Task DuplicatePnr_OrderNotificationReturnsErrors_LogsAcknowledgementCountAndRemovesQueueItem()
        {
            var analytics = new FakeTrackAnalyticsEventCommandHandler();
            var orderNotification = new FakeOrderNotificationMSCommand(new OrderNotificationResponse
            {
                Errors = new[] { new VAA.EAPI.Domain.Entities.Common.ErrorDetails { Code = "G009" } },
                OrderNotificationAgentResponse = new OrderNotificationAgentResponse
                {
                    AgentResponse = new[]
                    {
                        new AgentResponse
                        {
                            AgentId = "AG1",
                            Status = VAA.EAPI.Domain.Entities.Common.Status.Accepted,
                            AcknowledgementRcvd = true
                        }
                    }
                }
            });
            var fetch = new FakeNdcQueueMgmtFetchDuplicatePnrsCommand(new List<string> { MainPnr });
            var remove = new FakeNdcQueueMgmtRemoveDuplicatePnrsCommand();
            var logger = new RecordingLogger<OrderChangeDuplicatePNRNotifCommandHandler>();
            var httpConnector = new StubHttpConnector((uri, _, body) =>
            {
                if (uri.StartsWith(OrderDataEndpoint, StringComparison.OrdinalIgnoreCase))
                    return Ok(JsonOrderDataOrderDoc(uri.Substring(OrderDataEndpoint.Length)));

                if (uri.Is(OrderSearchUrl))
                {
                    var isMain = body != null && body.Contains(MainPnr, StringComparison.OrdinalIgnoreCase);
                    return Ok(JsonGetReservationDoc(isMain ? MainPnr : OtherPnr, "AG1", isMain));
                }

                if (uri.Is(ValidationUrl))
                    return Ok("{\"OCNFlag\":true}");

                if (uri.StartsWith(AgencyOcnDetailsUrl, StringComparison.OrdinalIgnoreCase))
                    return Ok("[{\"AgencyDetails\":{\"RequestAgencyId\":\"AG1\"},\"EndpointDetails\":{\"Endpoint\":\"https://agent/ocn\",\"EndpointId\":1}}]");

                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
            });

            ConfigureCommonSettings();
            var handler = BuildHandler(httpConnector, analytics, orderNotification, fetch, remove, logger);

            var result = await handler.Handle(new OrderChangeDuplicatePNRNotifRQ { UniqueId = MainPnr, PCC = "ABCD" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { MainPnr }, remove.RemovedPnrs);
            var analyticsRequest = Assert.Single(analytics.Requests);
            Assert.Equal(MainPnr, analyticsRequest.GetValue("PNR"));
            Assert.Equal("Failed", analyticsRequest.GetValue("OcnStatus"));
            Assert.Equal(1, Convert.ToInt32(analyticsRequest.GetValue("AgentAcknowledgementCount")));
            Assert.Equal("Removed", analyticsRequest.GetValue("QueueRemovalStatus"));
        }

        [Fact]
        public async Task DuplicatePnr_QueueRemovalFails_LogsQueueRemovalFailedReason()
        {
            var analytics = new FakeTrackAnalyticsEventCommandHandler();
            var orderNotification = new FakeOrderNotificationMSCommand(new OrderNotificationResponse
            {
                OrderNotificationAgentResponse = new OrderNotificationAgentResponse
                {
                    AgentResponse = new[]
                    {
                        new AgentResponse
                        {
                            AgentId = "AG1",
                            Status = VAA.EAPI.Domain.Entities.Common.Status.Accepted,
                            AcknowledgementRcvd = true
                        }
                    }
                }
            });
            var fetch = new FakeNdcQueueMgmtFetchDuplicatePnrsCommand(new List<string> { MainPnr });
            var remove = new FakeNdcQueueMgmtRemoveDuplicatePnrsCommand(returnsSuccess: false);
            var logger = new RecordingLogger<OrderChangeDuplicatePNRNotifCommandHandler>();
            var httpConnector = new StubHttpConnector((uri, _, body) =>
            {
                if (uri.StartsWith(OrderDataEndpoint, StringComparison.OrdinalIgnoreCase))
                    return Ok(JsonOrderDataOrderDoc(uri.Substring(OrderDataEndpoint.Length)));

                if (uri.Is(OrderSearchUrl))
                {
                    var isMain = body != null && body.Contains(MainPnr, StringComparison.OrdinalIgnoreCase);
                    return Ok(JsonGetReservationDoc(isMain ? MainPnr : OtherPnr, "AG1", isMain));
                }

                if (uri.Is(ValidationUrl))
                    return Ok("{\"OCNFlag\":true}");

                if (uri.StartsWith(AgencyOcnDetailsUrl, StringComparison.OrdinalIgnoreCase))
                    return Ok("[{\"AgencyDetails\":{\"RequestAgencyId\":\"AG1\"},\"EndpointDetails\":{\"Endpoint\":\"https://agent/ocn\",\"EndpointId\":1}}]");

                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
            });

            ConfigureCommonSettings();
            var handler = BuildHandler(httpConnector, analytics, orderNotification, fetch, remove, logger);

            var result = await handler.Handle(new OrderChangeDuplicatePNRNotifRQ { UniqueId = MainPnr, PCC = "ABCD" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { MainPnr }, remove.RemovedPnrs);
            var analyticsRequest = Assert.Single(analytics.Requests);
            Assert.Equal(MainPnr, analyticsRequest.GetValue("PNR"));
            Assert.Equal("Acknowledged", analyticsRequest.GetValue("OcnStatus"));
            Assert.Equal(1, Convert.ToInt32(analyticsRequest.GetValue("AgentAcknowledgementCount")));
            Assert.Equal("Failed", analyticsRequest.GetValue("QueueRemovalStatus"));
        }

        [Fact]
        public async Task DuplicatePnr_QueueRemovalThrows_LogsFailureWithException()
        {
            var analytics = new FakeTrackAnalyticsEventCommandHandler();
            var orderNotification = new FakeOrderNotificationMSCommand(new OrderNotificationResponse
            {
                OrderNotificationAgentResponse = new OrderNotificationAgentResponse
                {
                    AgentResponse = new[]
                    {
                        new AgentResponse
                        {
                            AgentId = "AG1",
                            Status = VAA.EAPI.Domain.Entities.Common.Status.Accepted,
                            AcknowledgementRcvd = true
                        }
                    }
                }
            });
            var fetch = new FakeNdcQueueMgmtFetchDuplicatePnrsCommand(new List<string> { MainPnr });
            var queueRemovalException = new Exception("Queue removal failed raw detail");
            var remove = new FakeNdcQueueMgmtRemoveDuplicatePnrsCommand(exception: queueRemovalException);
            var logger = new RecordingLogger<OrderChangeDuplicatePNRNotifCommandHandler>();
            var httpConnector = new StubHttpConnector((uri, _, body) =>
            {
                if (uri.StartsWith(OrderDataEndpoint, StringComparison.OrdinalIgnoreCase))
                    return Ok(JsonOrderDataOrderDoc(uri.Substring(OrderDataEndpoint.Length)));

                if (uri.Is(OrderSearchUrl))
                {
                    var isMain = body != null && body.Contains(MainPnr, StringComparison.OrdinalIgnoreCase);
                    return Ok(JsonGetReservationDoc(isMain ? MainPnr : OtherPnr, "AG1", isMain));
                }

                if (uri.Is(ValidationUrl))
                    return Ok("{\"OCNFlag\":true}");

                if (uri.StartsWith(AgencyOcnDetailsUrl, StringComparison.OrdinalIgnoreCase))
                    return Ok("[{\"AgencyDetails\":{\"RequestAgencyId\":\"AG1\"},\"EndpointDetails\":{\"Endpoint\":\"https://agent/ocn\",\"EndpointId\":1}}]");

                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
            });

            ConfigureCommonSettings();
            var handler = BuildHandler(httpConnector, analytics, orderNotification, fetch, remove, logger);

            var result = await handler.Handle(new OrderChangeDuplicatePNRNotifRQ { UniqueId = MainPnr, PCC = "ABCD" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { MainPnr }, remove.RemovedPnrs);
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR queue removal request failed. PCC=ABCD, PNR=PLRXW1");
            Assert.Contains(logger.Exceptions, exception => ReferenceEquals(queueRemovalException, exception));
        }

        [Fact]
        public async Task DuplicatePnr_WithDistributor_AppendsAggregatorIdToAgencyOcnDetailsRequest()
        {
            var analytics = new FakeTrackAnalyticsEventCommandHandler();
            var orderNotification = new FakeOrderNotificationMSCommand(new OrderNotificationResponse());
            var fetch = new FakeNdcQueueMgmtFetchDuplicatePnrsCommand(new List<string> { MainPnr });
            var remove = new FakeNdcQueueMgmtRemoveDuplicatePnrsCommand();
            var agencyOcnDetailsUris = new List<string>();
            const string aggregatorId = "AGGREGATOR1";
            var expectedAgencyOcnDetailsUrl = AgencyOcnDetailsUrl + "AG1?aggregatorId=" + aggregatorId;

            var httpConnector = new StubHttpConnector((uri, _, body) =>
            {
                if (uri.StartsWith(OrderDataEndpoint, StringComparison.OrdinalIgnoreCase))
                {
                    var pnr = uri.Substring(OrderDataEndpoint.Length);
                    return Ok(JsonOrderDataOrderDoc(pnr));
                }

                if (uri.Is(OrderSearchUrl))
                {
                    var isMain = body.Contains(MainPnr, StringComparison.OrdinalIgnoreCase);
                    return Ok(JsonGetReservationDoc(isMain ? MainPnr : OtherPnr, "AG1", isMain, distributorAgencyId: aggregatorId));
                }

                if (uri.Is(ValidationUrl))
                    return Ok("{\"OCNFlag\":true}");

                if (uri.StartsWith(AgencyOcnDetailsUrl, StringComparison.OrdinalIgnoreCase))
                {
                    agencyOcnDetailsUris.Add(uri);
                    return Ok("[{\"AgencyDetails\":{\"RequestAgencyId\":\"AG1\"},\"EndpointDetails\":{\"Endpoint\":\"https://agent/ocn\",\"EndpointId\":1}}]");
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
            });

            ConfigureCommonSettings();
            var handler = BuildHandler(httpConnector, analytics, orderNotification, fetch, remove);

            var result = await handler.Handle(new OrderChangeDuplicatePNRNotifRQ { UniqueId = MainPnr, PCC = "ABCD" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Contains(expectedAgencyOcnDetailsUrl, agencyOcnDetailsUris);
        }

        [Fact]
        public async Task DuplicatePnr_QueueFetchReturnsNoPnrs_ProcessesUniqueIdAndDoesNotRemoveIt()
        {
            var analytics = new FakeTrackAnalyticsEventCommandHandler();
            var orderNotification = new FakeOrderNotificationMSCommand(new OrderNotificationResponse());
            var fetch = new FakeNdcQueueMgmtFetchDuplicatePnrsCommand(Array.Empty<string>());
            var remove = new FakeNdcQueueMgmtRemoveDuplicatePnrsCommand();

            var httpConnector = new StubHttpConnector((uri, _, body) =>
            {
                if (uri.StartsWith(OrderDataEndpoint, StringComparison.OrdinalIgnoreCase))
                {
                    var pnr = uri.Substring(OrderDataEndpoint.Length);
                    return Ok(JsonOrderDataOrderDoc(pnr));
                }

                if (uri.Is(OrderSearchUrl))
                {
                    var isMain = body.Contains(MainPnr, StringComparison.OrdinalIgnoreCase);
                    return Ok(JsonGetReservationDoc(isMain ? MainPnr : OtherPnr, "AG1", isMain));
                }

                if (uri.Is(ValidationUrl))
                    return Ok("{\"OCNFlag\":true}");

                if (uri.StartsWith(AgencyOcnDetailsUrl, StringComparison.OrdinalIgnoreCase))
                    return Ok("[{\"AgencyDetails\":{\"RequestAgencyId\":\"AG1\"},\"EndpointDetails\":{\"Endpoint\":\"https://agent/ocn\",\"EndpointId\":1}}]");

                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
            });

            ConfigureCommonSettings();
            var handler = BuildHandler(httpConnector, analytics, orderNotification, fetch, remove);

            var result = await handler.Handle(new OrderChangeDuplicatePNRNotifRQ { UniqueId = MainPnr, PCC = "ABCD" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, orderNotification.CallCount);
            Assert.Empty(remove.RemovedPnrs);
        }

        [Fact]
        public async Task DuplicatePnr_DifferentAgentIds_SkipsOcn_AndRemovesMainQueueItem()
        {
            var analytics = new FakeTrackAnalyticsEventCommandHandler();
            var orderNotification = new FakeOrderNotificationMSCommand(new OrderNotificationResponse());
            var fetch = new FakeNdcQueueMgmtFetchDuplicatePnrsCommand(new List<string> { MainPnr });
            var remove = new FakeNdcQueueMgmtRemoveDuplicatePnrsCommand();

            var httpConnector = new StubHttpConnector((uri, method, body) =>
            {
                if (uri.StartsWith(OrderDataEndpoint, StringComparison.OrdinalIgnoreCase))
                {
                    var pnr = uri.Substring(OrderDataEndpoint.Length);
                    return Ok(JsonOrderDataOrderDoc(pnr));
                }

                if (uri.Is(OrderSearchUrl))
                {
                    var isMain = body != null && body.Contains(MainPnr, StringComparison.OrdinalIgnoreCase);
                    var pnr = isMain ? MainPnr : OtherPnr;

                    var agencyId = isMain ? "AG1" : "AG2";
                    return Ok(JsonGetReservationDoc(pnr, sellerAgencyId: agencyId, includeDuplicateRemark: isMain));
                }

                if (uri.Is(ValidationUrl))
                    return Ok("{\"OCNFlag\":true}");

                if (uri.StartsWith(AgencyOcnDetailsUrl, StringComparison.OrdinalIgnoreCase))
                    return Ok("[{\"AgencyDetails\":{\"RequestAgencyId\":\"AG1\"},\"EndpointDetails\":{\"Endpoint\":\"https://agent/ocn\",\"EndpointId\":1}}]");

                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
            });

            ConfigureCommonSettings();

            var handler = BuildHandler(httpConnector, analytics, orderNotification, fetch, remove);

            // Act
            var result = await handler.Handle(new OrderChangeDuplicatePNRNotifRQ { UniqueId = MainPnr, PCC = "ABCD" }, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            Assert.Equal(0, orderNotification.CallCount);
            Assert.Equal(new[] { MainPnr }, remove.RemovedPnrs);
            Assert.Equal(1, analytics.CallCount);
        }

        [Fact]
        public async Task DuplicatePnr_MultipleQueueItems_TracksAndRemovesEachQueueItem()
        {
            var analytics = new FakeTrackAnalyticsEventCommandHandler();
            var orderNotification = new FakeOrderNotificationMSCommand(new OrderNotificationResponse
            {
                OrderNotificationAgentResponse = new OrderNotificationAgentResponse
                {
                        AgentResponse = new[]
                        {
                            new AgentResponse
                            {
                                AgentId = "AG1",
                                Status = VAA.EAPI.Domain.Entities.Common.Status.Accepted,
                                AcknowledgementRcvd = true
                            }
                    }
                }
            });
            var fetch = new FakeNdcQueueMgmtFetchDuplicatePnrsCommand(new List<string> { MainPnr, MainPnr2 });
            var remove = new FakeNdcQueueMgmtRemoveDuplicatePnrsCommand();

            var httpConnector = new StubHttpConnector((uri, method, body) =>
            {
                if (uri.StartsWith(OrderDataEndpoint, StringComparison.OrdinalIgnoreCase))
                {
                    var pnr = uri.Substring(OrderDataEndpoint.Length);
                    return Ok(JsonOrderDataOrderDoc(pnr));
                }

                if (uri.Is(OrderSearchUrl))
                {
                    if (body != null && body.Contains(MainPnr, StringComparison.OrdinalIgnoreCase))
                        return Ok(JsonGetReservationDoc(MainPnr, sellerAgencyId: "AG1", includeDuplicateRemark: true, duplicatePnr: OtherPnr));

                    if (body != null && body.Contains(OtherPnr, StringComparison.OrdinalIgnoreCase))
                        return Ok(JsonGetReservationDoc(OtherPnr, sellerAgencyId: "AG1", includeDuplicateRemark: false, duplicatePnr: null));

                    if (body != null && body.Contains(MainPnr2, StringComparison.OrdinalIgnoreCase))
                        return Ok(JsonGetReservationDoc(MainPnr2, sellerAgencyId: "AG1", includeDuplicateRemark: true, duplicatePnr: OtherPnr2));

                    if (body != null && body.Contains(OtherPnr2, StringComparison.OrdinalIgnoreCase))
                        return Ok(JsonGetReservationDoc(OtherPnr2, sellerAgencyId: "AG2", includeDuplicateRemark: false, duplicatePnr: null));

                    return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
                }

                if (uri.Is(ValidationUrl))
                    return Ok("{\"OCNFlag\":true}");

                if (uri.StartsWith(AgencyOcnDetailsUrl, StringComparison.OrdinalIgnoreCase))
                    return Ok("[{\"AgencyDetails\":{\"RequestAgencyId\":\"AG1\"},\"EndpointDetails\":{\"Endpoint\":\"https://agent/ocn\",\"EndpointId\":1}}]");

                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
            });

            ConfigureCommonSettings();

            var handler = BuildHandler(httpConnector, analytics, orderNotification, fetch, remove);

            // Act
            var result = await handler.Handle(new OrderChangeDuplicatePNRNotifRQ { UniqueId = MainPnr, PCC = "ABCD" }, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            Assert.Equal(1, orderNotification.CallCount);
            Assert.Equal(new[] { MainPnr, MainPnr2 }, remove.RemovedPnrs);
            Assert.Equal(2, analytics.Requests.Count);
            Assert.NotSame(analytics.Requests[0], analytics.Requests[1]);
            Assert.Equal(MainPnr, analytics.Requests[0].GetValue("PNR"));
            Assert.Equal(MainPnr2, analytics.Requests[1].GetValue("PNR"));
        }

        [Fact]
        public async Task DuplicatePnr_OtherPnrLoadFails_WhenOtherPnrIsOnQueue_SkipsOcn_AndRemovesEachQueuePnr()
        {
            var analytics = new FakeTrackAnalyticsEventCommandHandler();
            var orderNotification = new FakeOrderNotificationMSCommand(new OrderNotificationResponse());
            var fetch = new FakeNdcQueueMgmtFetchDuplicatePnrsCommand(new List<string> { MainPnr, OtherPnr });
            var remove = new FakeNdcQueueMgmtRemoveDuplicatePnrsCommand();

            var httpConnector = new StubHttpConnector((uri, _, body) =>
            {
                if (uri.StartsWith(OrderDataEndpoint, StringComparison.OrdinalIgnoreCase))
                {
                    var pnr = uri.Substring(OrderDataEndpoint.Length);
                    return Ok(JsonOrderDataOrderDoc(pnr));
                }

                if (uri.Is(OrderSearchUrl))
                {
                    return body.Contains(MainPnr, StringComparison.OrdinalIgnoreCase)
                        ? Ok(JsonGetReservationDoc(MainPnr, sellerAgencyId: "AG1", includeDuplicateRemark: true))
                        : new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("[{}]") };
                }

                if (uri.Is(ValidationUrl))
                    return Ok("{\"OCNFlag\":true}");

                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
            });

            ConfigureCommonSettings();
            var handler = BuildHandler(httpConnector, analytics, orderNotification, fetch, remove);

            var result = await handler.Handle(new OrderChangeDuplicatePNRNotifRQ { UniqueId = MainPnr, PCC = "ABCD" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(0, orderNotification.CallCount);
            Assert.Equal(new[] { MainPnr, OtherPnr }, remove.RemovedPnrs);
        }

        [Fact]
        public async Task DuplicatePnr_OtherPnrLoadFails_WhenOtherPnrIsNotOnQueue_SkipsOcn_AndRemovesMainQueuePnrOnly()
        {
            var analytics = new FakeTrackAnalyticsEventCommandHandler();
            var orderNotification = new FakeOrderNotificationMSCommand(new OrderNotificationResponse());
            var fetch = new FakeNdcQueueMgmtFetchDuplicatePnrsCommand(new List<string> { MainPnr });
            var remove = new FakeNdcQueueMgmtRemoveDuplicatePnrsCommand();

            var httpConnector = new StubHttpConnector((uri, _, body) =>
            {
                if (uri.StartsWith(OrderDataEndpoint, StringComparison.OrdinalIgnoreCase))
                {
                    var pnr = uri.Substring(OrderDataEndpoint.Length);
                    return Ok(JsonOrderDataOrderDoc(pnr));
                }

                if (uri.Is(OrderSearchUrl))
                {
                    return body.Contains(MainPnr, StringComparison.OrdinalIgnoreCase)
                        ? Ok(JsonGetReservationDoc(MainPnr, sellerAgencyId: "AG1", includeDuplicateRemark: true))
                        : new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("[{}]") };
                }

                if (uri.Is(ValidationUrl))
                    return Ok("{\"OCNFlag\":true}");

                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") };
            });

            ConfigureCommonSettings();
            var handler = BuildHandler(httpConnector, analytics, orderNotification, fetch, remove);

            var result = await handler.Handle(new OrderChangeDuplicatePNRNotifRQ { UniqueId = MainPnr, PCC = "ABCD" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(0, orderNotification.CallCount);
            Assert.Equal(new[] { MainPnr }, remove.RemovedPnrs);
        }

        [Fact]
        public async Task QueueMgmtRemove_SendsPccAndTargetPnr()
        {
            string requestBody = null;
            var httpConnector = new StubHttpConnector((_, _, body) =>
            {
                requestBody = body;
                return Ok(string.Empty);
            });

            ConfigReader.Set(Constants.Environment.NdcQueueMgmtRemoveDuplicatePnrsUrl, "https://test/queuemgmt/remove");
            ConfigReader.Set(Constants.Environment.NdcQueueMgmtSubscriptionKey, "test-key");

            var handler = new NdcQueueMgmtRemoveDuplicatePnrsCommandHandler(
                httpConnector,
                NullLogger<NdcQueueMgmtRemoveDuplicatePnrsCommandHandler>.Instance);

            var result = await handler.Handle(new OrderChangeDuplicatePNRNotifRQ { PCC = "ABCD" }, OtherPnr, CancellationToken.None);

            Assert.True(result);
            Assert.Equal("{\"pcc\":\"ABCD\",\"pnr\":\"PLQHCB\"}", requestBody);
        }

        private static OrderChangeDuplicatePNRNotifCommandHandler BuildHandler(
            IHttpConnector httpConnector,
            ITrackAnalyticsEventCommandHandler analytics,
            IOrderNotificationMSCommand orderNotification,
            INdcQueueMgmtFetchDuplicatePnrsCommand fetch,
            INdcQueueMgmtRemoveDuplicatePnrsCommand remove,
            ILogger<OrderChangeDuplicatePNRNotifCommandHandler> logger = null)
        {
            var orderData = new OrderDataMSQueryHandler(httpConnector);
            var orderSearch = new OrderSearchMSCommandHandler(httpConnector);
            var validation = new ValidationMSCommandHandler(httpConnector);
            var agencyOcnDetails = new ValidationMSAgencyOCNDetailsQueryHandler(httpConnector);

            return new OrderChangeDuplicatePNRNotifCommandHandler(
                logger ?? NullLogger<OrderChangeDuplicatePNRNotifCommandHandler>.Instance,
                analytics,
                orderData,
                validation,
                agencyOcnDetails,
                orderSearch,
                orderNotification,
                fetch,
                remove);
        }

        private sealed class FakeTrackAnalyticsEventCommandHandler : ITrackAnalyticsEventCommandHandler
        {
            public int CallCount { get; private set; }
            public List<TrackAnalyticsEventCommand> Requests { get; } = new();

            public Task<bool> Handle(TrackAnalyticsEventCommand request, CancellationToken cancellationToken)
            {
                CallCount++;
                Requests.Add(request);
                return Task.FromResult(true);
            }
        }

        private sealed class RecordingLogger<T> : ILogger<T>
        {
            public List<string> Messages { get; } = new();
            public List<Exception?> Exceptions { get; } = new();

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => EmptyScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Messages.Add(formatter(state, exception));
                Exceptions.Add(exception);
            }

            private sealed class EmptyScope : IDisposable
            {
                public static EmptyScope Instance { get; } = new();

                public void Dispose()
                {
                }
            }
        }

        private sealed class FakeOrderNotificationMSCommand : IOrderNotificationMSCommand
        {
            private readonly OrderNotificationResponse _response;
            private readonly Exception? _exception;

            public FakeOrderNotificationMSCommand(OrderNotificationResponse response, Exception? exception = null)
            {
                _response = response;
                _exception = exception;
            }

            public int CallCount { get; private set; }
            public List<OrderNotificationRequest> Requests { get; } = new();

            public Task<OrderNotificationResponse> Handle(OrderNotificationRequest request)
            {
                CallCount++;
                Requests.Add(request);

                if (_exception != null)
                    throw _exception;

                return Task.FromResult(_response);
            }
        }

        private sealed class FakeNdcQueueMgmtFetchDuplicatePnrsCommand : INdcQueueMgmtFetchDuplicatePnrsCommand
        {
            private readonly IReadOnlyCollection<string> _pnrs;

            public FakeNdcQueueMgmtFetchDuplicatePnrsCommand(IReadOnlyCollection<string> pnrs)
            {
                _pnrs = pnrs;
            }

            public Task<IReadOnlyCollection<string>> Handle(OrderChangeDuplicatePNRNotifRQ request, CancellationToken cancellationToken) => Task.FromResult(_pnrs);
        }

        private sealed class FakeNdcQueueMgmtRemoveDuplicatePnrsCommand : INdcQueueMgmtRemoveDuplicatePnrsCommand
        {
            private readonly bool _returnsSuccess;
            private readonly Exception? _exception;

            public FakeNdcQueueMgmtRemoveDuplicatePnrsCommand(bool returnsSuccess = true, Exception? exception = null)
            {
                _returnsSuccess = returnsSuccess;
                _exception = exception;
            }

            public List<string> RemovedPnrs { get; } = new();

            public Task<bool> Handle(OrderChangeDuplicatePNRNotifRQ request, string pnr, CancellationToken cancellationToken)
            {
                RemovedPnrs.Add(pnr);

                if (_exception != null)
                    throw _exception;

                return Task.FromResult(_returnsSuccess);
            }
        }

        private static void ConfigureCommonSettings()
        {
            ConfigReader.Set(Constants.Environment.OrderDataMSEndpoint, OrderDataEndpoint);
            ConfigReader.Set(Constants.Environment.OrderDataMSSubscriptionKey, "123");

            ConfigReader.Set(Constants.Environment.OrderSearchMSUrl, OrderSearchUrl);
            ConfigReader.Set(Constants.Environment.OrderSearchMSSubscriptionKey, "123");

            ConfigReader.Set(Constants.Environment.ValidationMSUrl, ValidationUrl);
            ConfigReader.Set(Constants.Environment.ValidationMSSubscriptionKey, "123");
            ConfigReader.Set(Constants.Environment.AgentRegistrationEnvironment, "test");

            ConfigReader.Set(Constants.Environment.ValidationMSAgencyOCNDetailsUrl, AgencyOcnDetailsUrl);
        }

        private const string OrderDataEndpoint = "https://test/orderdata/orders/";
        private const string OrderSearchUrl = "https://test/ordersearch";
        private const string ValidationUrl = "https://test/validation";
        private const string AgencyOcnDetailsUrl = "https://test/validation/AgencyOCNDetails/";

        private static HttpResponseMessage Ok(string json) =>
            new(HttpStatusCode.OK) { Content = new StringContent(json) };

        private static string JsonOrderDataOrderDoc(string pnr) =>
            $"{{\"BookingReference\":{{\"RLOC\":\"{pnr}\"}},\"OrderNumber\":{{\"OrderId\":\"{pnr}\"}},\"DataList\":{{}},\"Parties\":{{\"Participants\":[{{\"PartyType\":\"Seller\",\"AgencyID\":\"AG1\"}}]}}}}";

        private static string JsonGetReservationDoc(string pnr, string sellerAgencyId, bool includeDuplicateRemark, string? duplicatePnr = null, string? distributorAgencyId = null)
        {
            var remarksJson = includeDuplicateRemark
                ? $"[{{\"Text\":\"POSSIBLE DUPE BOOKING. SEE PNR {duplicatePnr ?? OtherPnr}\"}}]"
                : "[]";
            var distributorJson = distributorAgencyId is null
                ? string.Empty
                : $",{{\"PartyType\":\"Distributor\",\"AgencyID\":\"{distributorAgencyId}\"}}";

            return $"{{\"BookingReference\":{{\"RLOC\":\"{pnr}\"}},\"OrderNumber\":{{\"OrderId\":\"{pnr}\"}},\"DataList\":{{}},\"Parties\":{{\"Participants\":[{{\"PartyType\":\"Seller\",\"AgencyID\":\"{sellerAgencyId}\"}}{distributorJson}]}},\"OrderRemarks\":{remarksJson}}}";
        }

        private sealed class StubHttpConnector : IHttpConnector
        {
            private readonly Func<string, HttpMethod, string, HttpResponseMessage> _handler;

            public StubHttpConnector(Func<string, HttpMethod, string, HttpResponseMessage> handler)
            {
                _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            }

            public Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, string content, bool preventFollow202Location = false)
                => Task.FromResult(_handler(uri, method, content ?? string.Empty));

            public Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, string content, CookieContainer cookies, bool preventFollow202Location = false)
                => SendAsync(uri, headers, bearerToken, method, content, preventFollow202Location);

            public Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, HttpContent content, bool preventFollow202Location = false)
     =>     SendAsync(uri, headers, bearerToken, method, content, CancellationToken.None, preventFollow202Location);

            public Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, HttpContent content, CookieContainer cookies, bool preventFollow202Location = false)
                => SendAsync(uri, headers, bearerToken, method, content, CancellationToken.None, preventFollow202Location);

            public async Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, HttpContent content, CancellationToken cancellationToken, bool preventFollow202Location = false)
            {
                var body = content is null ? string.Empty : await content.ReadAsStringAsync(cancellationToken);
                return _handler(uri, method, body ?? string.Empty);
            }

            public Task<T> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod verb, HttpContent content, bool preventFollow202Location = false)
                => throw new NotImplementedException();

            public Task<T> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod verb, HttpContent content, CookieContainer cookies, bool preventFollow202Location = false)
                => throw new NotImplementedException();

            public Task<T> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod verb, string content, bool preventFollow202Location = false)
                => throw new NotImplementedException();

            public Task<T> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod verb, string content, CookieContainer cookies, bool preventFollow202Location = false)
                => throw new NotImplementedException();

            public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => throw new NotImplementedException();

            public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CookieContainer cookies, CancellationToken cancellationToken)
                => throw new NotImplementedException();

            public Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
                => throw new NotImplementedException();

            public Task<T> SendAsync<T>(HttpRequestMessage request, CookieContainer cookies, CancellationToken cancellationToken)
                => throw new NotImplementedException();

            public Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string, string> headers, string bearerToken = null)
                => throw new NotImplementedException();

            public Task<T> GetAsync<T>(string uri, Dictionary<string, string> headers = null, string bearerToken = null)
                => throw new NotImplementedException();

            public Task<HttpResponseMessage> PostAsync(string uri, HttpContent content, Dictionary<string, string> headers = null, string bearerToken = null)
                => throw new NotImplementedException();

            public Task<T> PostAsync<T>(string uri, HttpContent content, Dictionary<string, string> headers = null, string bearerToken = null)
                => throw new NotImplementedException();

            public Task<T> DeleteAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken = null)
                => throw new NotImplementedException();

            public Task<T> PatchAsync<T>(string uri, HttpContent content, Dictionary<string, string> headers, string bearerToken = null)
                => throw new NotImplementedException();

            public Task<T> PutAsync<T>(string uri, HttpContent content, Dictionary<string, string> headers, string bearerToken = null)
                => throw new NotImplementedException();
        }
    }
}
