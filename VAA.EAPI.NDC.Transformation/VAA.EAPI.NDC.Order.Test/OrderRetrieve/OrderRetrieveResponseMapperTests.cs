using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderRetrieve;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Mapper.Order;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderRetrieve
{
    public class OrderRetrieveResponseMapperTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderRetrieveQueryHandler>> _logger;
        private readonly Mock<IHostEnvironment> _hostEnvironment;
        private readonly Mock<IGdsOrderResponseMapper> _gdsOrderResponseMapper;
        private readonly Mock<IBrandMSCommand> _command;
        private readonly Mock<IBrandMSPTCCommand> _commandPTC;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly OrderRetrieveQueryHandler _handler;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;

        public OrderRetrieveResponseMapperTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderRetrieveQueryHandler>>();
            _hostEnvironment = new Mock<IHostEnvironment>();
            _gdsOrderResponseMapper = new Mock<IGdsOrderResponseMapper>();
            _command = new Mock<IBrandMSCommand>();
            _commandPTC = new Mock<IBrandMSPTCCommand>();
            _httpConnector = new Mock<IHttpConnector>();
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _handler = new OrderRetrieveQueryHandler(_mediator.Object, _logger.Object, _hostEnvironment.Object, _gdsOrderResponseMapper.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object);
        }

        [Fact]
        public void Retrieve_ResponseMapper_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\GetReservationOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, false, false, false, false, null, null, false, false, null, true);

            var response = ((OrderViewResponseType[])result.Items).First();
            var paymentResponse = ((PaymentFunctionsOrderType[])result.PaymentFunctions).First();

            Assert.NotNull(response);
            Assert.Equal("VS932OQOJKS26", response.Order.First().OrderID);
            Assert.NotNull(paymentResponse);
            Assert.Equal("81720962", paymentResponse.PaymentProcessingSummary.PaymentID);
        }

        [Fact]
        public void OCNOrderRetrieveReponseMapper_Test()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OCNOrderDoc");
            var warnings = new List<NDCWarning>();
            bool isHoldBooking = false;
            bool isCashPayment = false;
            bool isEasyPayment = false;
            string approvalCode = string.Empty;

            var orderResponse = OrderResponseMapper.MapOrderResponse(orderDoc, warnings, isHoldBooking, false, false, false, null, null, isCashPayment, isEasyPayment, approvalCode);

            Assert.NotNull(orderResponse);

            var orderView = orderResponse.Items as IEnumerable<OrderViewResponseType>;
            Assert.NotNull(orderView);

            var order = orderView.FirstOrDefault()?.Order?.FirstOrDefault();
            Assert.NotNull(order);

            var orderItem = order.OrderItem?.FirstOrDefault();
            Assert.NotNull(orderItem);

            var sellerFollowUp = orderItem.SellerFollowUpAction?.ActionCode?.FirstOrDefault();

            Assert.Contains("ContactAirline", sellerFollowUp);

        }

        [Theory]
        [InlineData(ChangeTypeEnum.FlightDateTimeChange, true)]
        [InlineData(ChangeTypeEnum.FlightNumberChange, true)]
        [InlineData(ChangeTypeEnum.FlightRouteChange, true)]
        [InlineData(ChangeTypeEnum.FlightCancellation, true)]
        [InlineData(ChangeTypeEnum.FlightDateTimeChange, false)]
        public void Retrieve_DisruptedOrderChange_ReturnsOldFareForReplacementPrice(ChangeTypeEnum changeType, bool isZeroValueReplacement)
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OCNOrderDoc");
            var disruptedOrderItem = orderDoc.OrderItems.First(orderItem => orderItem.OCNTimestamp != null);
            var ticketedOrderItem = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\GetReservationOrderDoc").OrderItems.First();
            var ticketedOrderItemCopy = Json.Deserialize<VAA.EAPI.Domain.Virgin.Models.Order.OrderItems>(Json.Serialize(ticketedOrderItem));
            const string replacementPaymentReferenceId = "replacement-payment";
            const string replacementTicketNumber = "9322520124224";
            const string originalTicketNumber = "9322520124223";
            const string otPaymentReferenceId = "OT";

            disruptedOrderItem.PaymentDetails = ticketedOrderItemCopy.PaymentDetails;
            disruptedOrderItem.AccountableDocumentRefs = ticketedOrderItemCopy.AccountableDocumentRefs;
            var replacementOrderItem = Json.Deserialize<VAA.EAPI.Domain.Virgin.Models.Order.OrderItems>(Json.Serialize(disruptedOrderItem));
            replacementOrderItem.OriginalSalePrice = Json.Deserialize<IEnumerable<VAA.EAPI.Domain.Virgin.Models.Common.Price>>(Json.Serialize(disruptedOrderItem.PriceDetails));
            var expectedOldTotalAmount = Convert.ToDecimal(replacementOrderItem.OriginalSalePrice.First(price => price.PriceComponentType == VAA.EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount);
            var expectedOldBaseAmount = Convert.ToDecimal(replacementOrderItem.OriginalSalePrice.First(price => price.PriceComponentType == VAA.EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount);
            var expectedOldTaxAmount = Convert.ToDecimal(replacementOrderItem.OriginalSalePrice.First(price => price.PriceComponentType == VAA.EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount);
            const decimal expectedOldOrderTotal = 1356.30m;
            var replacedPaymentReferenceIds = disruptedOrderItem.PaymentDetails.Select(paymentDetail => paymentDetail.PaymentReferenceId).ToArray();
            var replacedTicketNumbers = disruptedOrderItem.AccountableDocumentRefs.Select(accountableDocumentRef => accountableDocumentRef.DocumentNumber).ToArray();

            replacementOrderItem.OrderItemId = $"{disruptedOrderItem.OrderItemId}A";
            replacementOrderItem.ExistingOrderItemId = disruptedOrderItem.OrderItemId;
            replacementOrderItem.PriceDetails.First(price => price.PriceComponentType == VAA.EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount = isZeroValueReplacement ? "0.00" : "999.00";
            replacementOrderItem.PriceDetails.First(price => price.PriceComponentType == VAA.EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount = isZeroValueReplacement ? "0.00" : "888.00";
            replacementOrderItem.PriceDetails.First(price => price.PriceComponentType == VAA.EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount = isZeroValueReplacement ? "0.00" : "111.00";
            replacementOrderItem.PaymentDetails.First().PaymentReferenceId = replacementPaymentReferenceId;
            replacementOrderItem.PaymentDetails = replacementOrderItem.PaymentDetails.Append(new VAA.EAPI.Domain.Virgin.Models.Payment.PaymentDetails
            {
                Amount = new VAA.EAPI.Domain.Virgin.Models.Common.CurrencyAmount { MonetaryAmount = "0.00", BaseCurrency = "GBP" },
                PaymentCommitmentDateTime = "2026-01-01T00:00:00",
                PaymentProcessingStatus = VAA.EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL,
                PaymentReferenceId = otPaymentReferenceId
            });
            foreach (var accountableDocumentRef in replacementOrderItem.AccountableDocumentRefs)
            {
                accountableDocumentRef.DocumentNumber = replacementTicketNumber;
                accountableDocumentRef.OriginalDocumentNumber = originalTicketNumber;
                accountableDocumentRef.OriginalIssueDate = "2025-01-01T00:00:00";
                foreach (var coupon in accountableDocumentRef.Coupons)
                {
                    coupon.TicketNumber = replacementTicketNumber;
                }
            }
            orderDoc.OrderItems = orderDoc.OrderItems.Append(replacementOrderItem);
            orderDoc.ChangeType = changeType;

            var result = OrderResponseMapper.MapOrderRetrieveResponse(orderDoc, null, new OrderRetrieveResponseMapOptions
            {
                IsDisruptedOrderRetrieve = true
            });
            var orderViewResponse = ((OrderViewResponseType[])result.Items).Single();
            var order = orderViewResponse.Order.Single();
            var orderItems = order.OrderItem;
            var replacementOrderViewItem = orderItems.Single(orderItem => orderItem.OrderItemID == replacementOrderItem.OrderItemId);
            var paymentFunctions = (PaymentFunctionsOrderType[])result.PaymentFunctions;
            var paymentFunction = Assert.Single(paymentFunctions.Where(payment => payment.PaymentProcessingSummary.PaymentID == replacementPaymentReferenceId));
            var otPaymentFunction = Assert.Single(paymentFunctions.Where(payment => payment.PaymentProcessingSummary.PaymentID == otPaymentReferenceId));
            var ticketDocInfos = orderViewResponse.TicketDocInfo;

            Assert.DoesNotContain(orderItems, orderItem => orderItem.OrderItemID == disruptedOrderItem.OrderItemId);
            Assert.Null(replacementOrderViewItem.SellerFollowUpAction);
            Assert.NotNull(replacementOrderViewItem.Price);
            Assert.NotNull(replacementOrderViewItem.FareDetail);
            Assert.Equal(expectedOldTotalAmount, replacementOrderViewItem.Price.TotalAmount.Value);
            Assert.Equal(expectedOldBaseAmount, replacementOrderViewItem.Price.BaseAmount.Value);
            Assert.Equal(expectedOldTaxAmount, replacementOrderViewItem.Price.TaxSummary.Single().TotalTaxAmount.Value);
            Assert.Equal(expectedOldOrderTotal, order.TotalPrice.TotalAmount.Value);
            Assert.Equal(replacementPaymentReferenceId, paymentFunction.PaymentProcessingSummary.PaymentID);
            Assert.Equal(new[] { replacementOrderItem.OrderItemId }, paymentFunction.OrderAssociation.OrderItemRefID);
            var offlinePayment = Assert.IsType<OfflinePaymentInstructionReceiptType>(otPaymentFunction.PaymentProcessingSummary.PaymentProcessingSummaryPaymentMethod.Item);
            Assert.Equal("OT", offlinePayment.PaymentTypeCode);
            Assert.NotEmpty(ticketDocInfos);
            Assert.Contains(ticketDocInfos, ticketDocInfo => ticketDocInfo.Ticket.Single().TicketNumber == replacementTicketNumber &&
                ticketDocInfo.PaymentRefID?.Contains(replacementPaymentReferenceId) == true &&
                ticketDocInfo.OriginalIssueInfo?.TicketNumber == originalTicketNumber);
            Assert.DoesNotContain(ticketDocInfos, ticketDocInfo =>
                ticketDocInfo.PaymentRefID?.Any(replacedPaymentReferenceIds.Contains) == true ||
                ticketDocInfo.Ticket?.Any(ticket => replacedTicketNumbers.Contains(ticket.TicketNumber)) == true);
        }

        [Fact]
        public void Retrieve_PaidSeatResponseMapper_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\PaidSeatOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, false, false, false, false, null, null, false, false, null, true);

            var response = ((OrderViewResponseType[])result.Items).First();
            var paymentResponse = ((PaymentFunctionsOrderType[])result.PaymentFunctions).First();

            Assert.NotNull(response);
            Assert.Equal("VS932PPEQRV26", response.Order.First().OrderID);

            var ticketDocInfoJ = response.TicketDocInfo.FirstOrDefault(t => t.Ticket?.First().TicketDocTypeCode == "J");

            Assert.NotNull(ticketDocInfoJ); 

            // Now validate ReferencedOrder for that specific ticket
            Assert.NotNull(ticketDocInfoJ.ReferencedOrder);
            Assert.Equal("VS932PPEQRV26", ticketDocInfoJ.ReferencedOrder.OrderID);
            Assert.Equal("VS", ticketDocInfoJ.ReferencedOrder.OwnerCode);

            Assert.Contains(ticketDocInfoJ.ReferencedOrder.OrderItem,item => item.OrderItemRefID == "VS932PPEQRV26-3");
        }

        [Fact]
        public void HoldBooking_UN_Service_Gives_Delivery_REMOVED_And_Service_CANCELLED()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\GetReservationOrderDoc");
            orderDoc.OrderItems.First().AccountableDocumentRefs = null;
            orderDoc.DataList.SegmentDetails.First().Status = new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Status { Code = "UN" };

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, true, false, false, false, null, null, false, false, null, true);
            var orderView = ((OrderViewResponseType[])result.Items).First();

            var segmentId = orderDoc.DataList.SegmentDetails.First().SegmentId;
            var service = orderView.Order.First().OrderItem.First().Service
                .First(s => s.OrderServiceAssociation?.Item is PaxSegmentRefType paxSeg && paxSeg.PaxSegmentRefID == segmentId);

            Assert.Equal(Constants.Application.DeliveryStatusCodeRemoved, service.DeliveryStatusCode);
            Assert.Equal(Constants.Application.ServiceStatusCodeCancelled, service.StatusCode);
        }

        [Fact]
        public void HoldBooking_TK_Service_Gives_Delivery_CONFIRMED_And_Service_CONFIRMED()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\GetReservationOrderDoc");
            orderDoc.OrderItems.First().AccountableDocumentRefs = null;
            orderDoc.DataList.SegmentDetails.First().Status = new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Status { Code = "TK" };

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, true, false, false, false, null, null, false, false, null, true);
            var orderView = ((OrderViewResponseType[])result.Items).First();

            var order = orderView.Order.First();
            var orderItem = order.OrderItem.First();
            var segmentId = orderDoc.DataList.SegmentDetails.First().SegmentId;
            var service = orderItem.Service
                .First(s => s.OrderServiceAssociation?.Item is PaxSegmentRefType paxSeg && paxSeg.PaxSegmentRefID == segmentId);

            Assert.Equal(Constants.Application.DeliveryStatusCodeConfirmed, service.DeliveryStatusCode);
            Assert.Equal(Constants.Application.DeliveryStatusCodeConfirmed, service.StatusCode);
            Assert.Equal(Constants.Application.OrderItemStatusCodeActive, orderItem.StatusCode);
            Assert.Equal(Constants.Application.OrderItemStatusCodeOpened, order.StatusCode);
        }

        [Fact]
        public void HoldBooking_CancellationOnly_WithOCN_And_UN_Remains_OrderItem_ACTIVE_And_Order_OPENED()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\GetReservationOrderDoc");
            orderDoc.OrderItems.First().AccountableDocumentRefs = null;
            orderDoc.DataList.SegmentDetails.First().Status = new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Status { Code = "UN" };
            orderDoc.OrderItems.First().OCNTimestamp = "2026-08-25T00:00:00Z";

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, true, false, false, false, null, null, false, false, null, true);
            var orderView = ((OrderViewResponseType[])result.Items).First();

            var order = orderView.Order.First();
            var orderItem = order.OrderItem.First();

            Assert.Equal(Constants.Application.OrderItemStatusCodeActive, orderItem.StatusCode);
            Assert.Equal(Constants.Application.OrderItemStatusCodeOpened, order.StatusCode);
        }

        [Fact]
        public void HoldBooking_CancellationPlusRebook_Returns_Both_UN_And_TK_Services_Correctly()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\GetReservationOrderDoc");
            orderDoc.OrderItems.First().AccountableDocumentRefs = null;

            var cancelledSegment = orderDoc.DataList.SegmentDetails.First();
            cancelledSegment.Status = new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Status { Code = "UN" };

            var rebookedSegment = new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment
            {
                OdRefId = cancelledSegment.OdRefId,
                SegmentId = "SEG2",
                AirCraft = cancelledSegment.AirCraft,
                MarketingCarrier = cancelledSegment.MarketingCarrier,
                MarketingFlightNumber = "104",
                OriginAirportCode = cancelledSegment.OriginAirportCode,
                DestinationAirportCode = "JFK",
                ClassOfService = cancelledSegment.ClassOfService,
                ScheduledArrivalTime = cancelledSegment.ScheduledArrivalTime,
                ScheduledDepartureTime = cancelledSegment.ScheduledDepartureTime,
                FlightDuration = cancelledSegment.FlightDuration,
                Legs = cancelledSegment.Legs,
                CabinType = cancelledSegment.CabinType,
                FareBasis = cancelledSegment.FareBasis,
                Status = new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Status { Code = "TK" },
                TicketNotValidAfter = cancelledSegment.TicketNotValidAfter,
                TicketNotValidBefore = cancelledSegment.TicketNotValidBefore
            };

            var segments = orderDoc.DataList.SegmentDetails.ToList();
            segments.Add(rebookedSegment);
            orderDoc.DataList.SegmentDetails = segments;

            var od = orderDoc.DataList.OriginDestinationAssociations.First();
            od.FlightSegmentRefIds = new[] { cancelledSegment.SegmentId, rebookedSegment.SegmentId };

            var orderItem = orderDoc.OrderItems.First();
            var cancelledService = orderItem.Services.First();

            cancelledService.SegmentRefIds = new[] { cancelledSegment.SegmentId };

            var rebookedService = new VAA.EAPI.Domain.Virgin.Models.Order.OrderService
            {
                ServiceId = "SV2",
                ParentServiceRefId = cancelledService.ParentServiceRefId,
                Product = cancelledService.Product,
                ProductSegmentAssociation = cancelledService.ProductSegmentAssociation,
                CurrentSeatAssignment = cancelledService.CurrentSeatAssignment,
                CurrentSeatAssignmentType = cancelledService.CurrentSeatAssignmentType,
                ServiceBeneficiaries = cancelledService.ServiceBeneficiaries,
                OrderInstructionRefs = cancelledService.OrderInstructionRefs,
                PriceDetails = cancelledService.PriceDetails,
                Status = cancelledService.Status,
                CustomerRefIds = cancelledService.CustomerRefIds,
                OriginDestinationRefIds = cancelledService.OriginDestinationRefIds,
                SegmentRefIds = new[] { rebookedSegment.SegmentId },
                SeatInformation = cancelledService.SeatInformation,
                BaggageRefIds = cancelledService.BaggageRefIds,
                PenaltyRef = cancelledService.PenaltyRef,
                PriceClassRefIds = cancelledService.PriceClassRefIds,
                ServiceDefinitionRefIds = cancelledService.ServiceDefinitionRefIds
            };

            var services = orderItem.Services.ToList();
            services.Add(rebookedService);
            orderItem.Services = services;

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, true, false, false, false, null, null, false, false, null, true);
            var orderView = ((OrderViewResponseType[])result.Items).First();

            var mappedOrder = orderView.Order.First();
            var mappedOrderItem = mappedOrder.OrderItem.First();
            var mappedServices = mappedOrderItem.Service;

            var mappedUnService = mappedServices
                .First(s => s.OrderServiceAssociation?.Item is PaxSegmentRefType paxSeg && paxSeg.PaxSegmentRefID == cancelledSegment.SegmentId);
            var mappedTkService = mappedServices
                .First(s => s.OrderServiceAssociation?.Item is PaxSegmentRefType paxSeg && paxSeg.PaxSegmentRefID == rebookedSegment.SegmentId);

            Assert.Equal(Constants.Application.DeliveryStatusCodeRemoved, mappedUnService.DeliveryStatusCode);
            Assert.Equal(Constants.Application.ServiceStatusCodeCancelled, mappedUnService.StatusCode);

            Assert.Equal(Constants.Application.DeliveryStatusCodeConfirmed, mappedTkService.DeliveryStatusCode);
            Assert.Equal(Constants.Application.DeliveryStatusCodeConfirmed, mappedTkService.StatusCode);

            Assert.Equal(Constants.Application.OrderItemStatusCodeActive, mappedOrderItem.StatusCode);
            Assert.Equal(Constants.Application.OrderItemStatusCodeOpened, mappedOrder.StatusCode);
        }

        [Fact]
        public void HoldBooking_Manual_SellerFollowUpAction_Is_Returned_Unchanged()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\GetReservationOrderDoc");
            orderDoc.OrderItems.First().AccountableDocumentRefs = null;
            orderDoc.OrderItems.First().SellerFollowUpAction = new[] { "ManualAction" };
            orderDoc.DataList.SegmentDetails.First().Status = new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Status { Code = "UN" };

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, true, false, false, false, null, null, false, false, null, true);
            var orderView = ((OrderViewResponseType[])result.Items).First();

            var sellerFollowUp = orderView.Order.First().OrderItem.First().SellerFollowUpAction;

            Assert.NotNull(sellerFollowUp);
            Assert.Contains("ManualAction", sellerFollowUp.ActionCode);
            Assert.DoesNotContain("ContactAirline", sellerFollowUp.ActionCode);
        }

        [Fact]
        public void HoldBooking_NewOrderItemId_And_NewSegmentDetails_Appear_In_Response_DataLists()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\GetReservationOrderDoc");
            orderDoc.OrderItems.First().AccountableDocumentRefs = null;

            var baseSegment = orderDoc.DataList.SegmentDetails.First();
            var newSegment = new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment
            {
                OdRefId = baseSegment.OdRefId,
                SegmentId = "SEG2",
                AirCraft = baseSegment.AirCraft,
                MarketingCarrier = baseSegment.MarketingCarrier,
                MarketingFlightNumber = "999",
                OriginAirportCode = "LHR",
                DestinationAirportCode = "JFK",
                ClassOfService = baseSegment.ClassOfService,
                ScheduledArrivalTime = baseSegment.ScheduledArrivalTime,
                ScheduledDepartureTime = baseSegment.ScheduledDepartureTime,
                FlightDuration = baseSegment.FlightDuration,
                Legs = baseSegment.Legs,
                CabinType = baseSegment.CabinType,
                FareBasis = baseSegment.FareBasis,
                Status = new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Status { Code = "TK" },
                TicketNotValidAfter = baseSegment.TicketNotValidAfter,
                TicketNotValidBefore = baseSegment.TicketNotValidBefore
            };

            var segments = orderDoc.DataList.SegmentDetails.ToList();
            segments.Add(newSegment);
            orderDoc.DataList.SegmentDetails = segments;

            var orderItem = orderDoc.OrderItems.First();
            orderItem.OrderItemId = "1A";
            orderItem.Services.First().SegmentRefIds = new[] { newSegment.SegmentId };

            var od = orderDoc.DataList.OriginDestinationAssociations.First();
            od.FlightSegmentRefIds = new[] { newSegment.SegmentId };

            var journey = orderDoc.DataList.Journey.First();
            journey.FlightSegmentRefIds = new[] { newSegment.SegmentId };

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, true, false, false, false, null, null, false, false, null, true);
            var orderView = ((OrderViewResponseType[])result.Items).First();

            Assert.Equal("1A", orderView.Order.First().OrderItem.First().OrderItemID);

            var mappedService = orderView.Order.First().OrderItem.First().Service
                .First(s => s.OrderServiceAssociation?.Item is PaxSegmentRefType paxSeg && paxSeg.PaxSegmentRefID == newSegment.SegmentId);
            Assert.Equal(Constants.Application.DeliveryStatusCodeConfirmed, mappedService.DeliveryStatusCode);
            Assert.Equal(Constants.Application.DeliveryStatusCodeConfirmed, mappedService.StatusCode);

            Assert.NotNull(orderView.DataLists.DatedMarketingSegmentList);
            Assert.Contains(orderView.DataLists.DatedMarketingSegmentList, dms =>
                dms.Dep.IATA_LocationCode == "LHR" &&
                dms.Arrival.IATA_LocationCode == "JFK" &&
                dms.MarketingCarrierFlightNumberText == "999");
        }
    }
}
