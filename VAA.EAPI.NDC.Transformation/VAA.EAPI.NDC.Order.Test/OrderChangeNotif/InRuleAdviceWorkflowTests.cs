using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using DomainCustomer = VAA.EAPI.Domain.Virgin.Models.Customer.Customer;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChangeNotif;
using VAA.EAPI.NDC.Order.Infrastructure.Model.InRule;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using OrderMsDataList = VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.DataList;
using VAA.EAPI.NDC.Order.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.InRule;
using VAA.EAPI.NDC.Order.Tests.Infrastructure.Services.InRule;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderNotificationMS;
using Xunit;

namespace VAA.EAPI.NDC.Order.Tests.OrderChangeNotif
{
    [Collection(InRuleConfigurationCollection.Name)]
    public class InRuleAdviceWorkflowTests : IClassFixture<HttpClientFactoryFixture>
    {
        private const string InRuleUrl = "https://Testresources.virginatlantic.com/dev/inrule/disruptionrules";
        private const string SuccessCorrelationId = "fef7a05d-1937-45dc-982d-4f5f6757338d";
        private const string TimeoutCorrelationId = "d936302c-35ae-4c64-a876-eac67a43f7cd";
        private const string MockRequestUrl = InRuleUrl;
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;

        public InRuleAdviceWorkflowTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _httpClientFactoryFixture = httpClientFactoryFixture;
        }

        [Fact]
        public void CreateInRuleRequest_SerializesOnlyTheRequiredRulesEngineRequestContract()
        {
            var originalOrderDocument = CreateOrderDocument(segmentOneChanged: false, segmentTwoChanged: false);
            var refreshedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);

            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(originalOrderDocument, refreshedOrderDocument, "correlation-minor");
            var serializedRequest = Json.Serialize(request);
            var serializedRequestObject = Json.Deserialize<JObject>(serializedRequest);

            var order = Assert.Single(request.Order);
            Assert.Equal("VS932OWWGYG26", order.OrderNumber.OrderId);
            Assert.Equal("OWWGYG", order.BookingReference.RLOC);
            Assert.NotNull(order.DataList.OriginDestinationAssociations);
            Assert.NotNull(order.DataList.CustomerDetails);
            Assert.NotNull(order.DataList.OrderInstructions);
            Assert.NotNull(order.DataList.SegmentDetails);
            Assert.NotNull(order.DataList.Penalty);
            Assert.NotNull(order.DataList.PriceClass);
            Assert.NotNull(order.DataList.ServiceDefinitions);
            Assert.NotNull(order.DataList.Journey);
            Assert.NotNull(order.Parties);
            var serializedOrder = Assert.IsType<JObject>(serializedRequestObject["order"]?.Single());
            var serializedDataList = Assert.IsType<JObject>(serializedOrder["dataList"]);
            Assert.Contains("\"event\"", serializedRequest);
            Assert.Contains("\"order\"", serializedRequest);
            Assert.Contains("\"dataList\"", serializedRequest);
            Assert.Contains("\"segmentDetails\"", serializedRequest);
            Assert.NotNull(serializedDataList.Property("originDestinationAssociations"));
            Assert.NotNull(serializedDataList.Property("customerDetails"));
            Assert.NotNull(serializedDataList.Property("orderInstructions"));
            Assert.NotNull(serializedDataList.Property("segmentDetails"));
            Assert.NotNull(serializedDataList.Property("penalty"));
            Assert.NotNull(serializedDataList.Property("priceClass"));
            Assert.NotNull(serializedDataList.Property("serviceDefinitions"));
            Assert.NotNull(serializedDataList.Property("journey"));
            Assert.NotNull(serializedOrder.Property("pointOfSale"));
            Assert.NotNull(serializedOrder.Property("creationDateTime"));
            Assert.NotNull(serializedOrder.Property("orderNumber"));
            Assert.NotNull(serializedOrder.Property("bookingReference"));
            Assert.NotNull(serializedOrder.Property("orderItems"));
            Assert.NotNull(serializedOrder.Property("parties"));
            Assert.NotNull(serializedOrder.Property("changeType"));
            Assert.NotNull(serializedOrder.Property("changeDateTime"));
            Assert.DoesNotContain("eventName", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.Null(serializedRequestObject.Property("orderId"));
            Assert.Null(serializedRequestObject.Property("bookingReference"));
            Assert.Null(serializedRequestObject.Property("correlationId"));
            Assert.Null(serializedRequestObject.Property("originalItinerary"));
            Assert.Null(serializedRequestObject.Property("updatedItinerary"));
            Assert.Null(serializedRequestObject.Property("impactedSegments"));
            Assert.Null(serializedRequestObject.Property("scheduleChangeScenario"));
            Assert.DoesNotContain("paymentDetails", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("cardNumber", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("cvv", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("baggageAllowances", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("contacts", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("travelDocuments", serializedRequest, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateInRuleRequest_DoesNotSerializePaymentDetailsFromOrderItems()
        {
            const string cardNumber = "test-card-number-should-not-leak";
            const string cvv = "test-cvv-should-not-leak";
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            var sourceOrderItem = updatedOrderDocument.OrderItems.First();
            sourceOrderItem.PaymentDetails =
            [
                new PaymentDetails
                {
                    CardMethod = new CardDetails
                    {
                        CardNumber = cardNumber,
                        Cvv = cvv
                    }
                }
            ];

            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(null, updatedOrderDocument, "correlation-payment-details");
            var serializedRequest = Json.Serialize(request);
            var mappedOrderItem = Assert.Single(Assert.Single(request.Order).OrderItems.Where(orderItem =>
                orderItem.OrderItemId == sourceOrderItem.OrderItemId));

            Assert.NotNull(sourceOrderItem.PaymentDetails);
            Assert.Equal(cardNumber, sourceOrderItem.PaymentDetails.Single().CardMethod.CardNumber);
            Assert.NotSame(sourceOrderItem, mappedOrderItem);
            Assert.Equal(sourceOrderItem.OrderItemId, mappedOrderItem.OrderItemId);
            Assert.Null(typeof(OrderItem).GetProperty(nameof(PaymentDetails)));
            Assert.DoesNotContain("paymentDetails", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("cardNumber", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("cvv", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(cardNumber, serializedRequest, StringComparison.Ordinal);
            Assert.DoesNotContain(cvv, serializedRequest, StringComparison.Ordinal);
        }

        [Fact]
        public void CreateInRuleRequest_SerializesTheExpectedCustomerFieldsWithoutSensitiveDocuments()
        {
            const string firstName = "David";
            const string lastName = "Smith";
            const string email = "david.smith@example.com";
            const string address = "address-should-not-leak";
            const string loyaltyNumber = "loyalty-number-should-not-leak";
            const string travelDocumentNumber = "travel-document-should-not-leak";
            const string governmentDocumentNumber = "government-document-should-not-leak";
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            var sourceCustomer = new DomainCustomer
            {
                CustId = "PAX1",
                Ptc = "ADT",
                ItaPtc = "ADT",
                FirstName = firstName,
                LastName = lastName,
                Contacts = new Contacts { Email = email },
                FirstNameNumber = "01",
                LastNameNumber = "01",
                IsNameChanged = false,
                IsApisAdded = false,
                IsEmailUpdates = false,
                Addresses = new Address { AddressLine1 = address },
                LoyaltyMemberDetails = new LoyaltyMemberDetails { MemberNumber = loyaltyNumber },
                TravelDocuments = [new TravelDocument { DocumentNumber = travelDocumentNumber }],
                GovtIssuedDocs = [new GovtIssuedDoc { DocumentNumber = governmentDocumentNumber }]
            };
            updatedOrderDocument.DataList.CustomerDetails = [sourceCustomer];

            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(null, updatedOrderDocument, "correlation-customer-details");
            var serializedRequest = Json.Serialize(request);
            var serializedRequestObject = Json.Deserialize<JObject>(serializedRequest);
            var serializedCustomer = Assert.IsType<JObject>(serializedRequestObject["order"]?.Single()?["dataList"]?["customerDetails"]?.Single());

            var mappedCustomer = Assert.Single(Assert.Single(request.Order).DataList.CustomerDetails);
            Assert.Equal("PAX1", mappedCustomer.CustId);
            Assert.Equal("ADT", mappedCustomer.Ptc);
            Assert.Equal("ADT", mappedCustomer.ItaPtc);
            Assert.Equal(firstName, mappedCustomer.FirstName);
            Assert.Equal(lastName, mappedCustomer.LastName);
            Assert.Equal(email, mappedCustomer.Contacts.Email);
            Assert.Equal(
                ["custId", "ptc", "itaPtc", "firstName", "lastName", "contacts", "firstNameNumber", "lastNameNumber", "isNameChanged", "isApisAdded", "isEmailUpdates"],
                serializedCustomer.Properties().Select(property => property.Name));
            Assert.Equal(firstName, sourceCustomer.FirstName);
            Assert.Equal(email, sourceCustomer.Contacts.Email);
            Assert.Equal(travelDocumentNumber, sourceCustomer.TravelDocuments.Single().DocumentNumber);
            Assert.Equal(governmentDocumentNumber, sourceCustomer.GovtIssuedDocs.Single().DocumentNumber);
            Assert.DoesNotContain("addresses", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("loyaltyMemberDetails", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("travelDocuments", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("govtIssuedDocs", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(address, serializedRequest, StringComparison.Ordinal);
            Assert.DoesNotContain(loyaltyNumber, serializedRequest, StringComparison.Ordinal);
            Assert.DoesNotContain(travelDocumentNumber, serializedRequest, StringComparison.Ordinal);
            Assert.DoesNotContain(governmentDocumentNumber, serializedRequest, StringComparison.Ordinal);
        }

        [Fact]
        public void CreateInRuleRequest_OnlySerializesApprovedPointOfSaleFields()
        {
            const string deviceInfo = "device-info-should-not-leak";
            const string ipAddress = "192.0.2.1";
            const string browserInfo = "browser-info-should-not-leak";
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            var sourcePointOfSale = new Pos
            {
                CityCode = "LON",
                CountryCode = "GB",
                CurrencyCode = "GBP",
                DefaultCityCode = "LHR",
                CurrencyNumber = "826",
                NDCCityCodeLocation = "GBLON",
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress,
                BrowserInfo = browserInfo
            };
            updatedOrderDocument.PointOfSale = sourcePointOfSale;

            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(null, updatedOrderDocument, "correlation-point-of-sale");
            var serializedRequest = Json.Serialize(request);
            var serializedRequestObject = Json.Deserialize<JObject>(serializedRequest);
            var serializedPointOfSale = Assert.IsType<JObject>(serializedRequestObject["order"]?.Single()?["pointOfSale"]);

            var mappedPointOfSale = Assert.IsType<PointOfSale>(Assert.Single(request.Order).PointOfSale);
            Assert.Equal("LON", mappedPointOfSale.CityCode);
            Assert.Equal("GB", mappedPointOfSale.CountryCode);
            Assert.Equal("GBP", mappedPointOfSale.CurrencyCode);
            Assert.Equal(["cityCode", "countryCode", "currencyCode", "defaultCityCode", "currencyNumber", "ndcCityCodeLocation"], serializedPointOfSale.Properties().Select(property => property.Name));
            Assert.Equal(deviceInfo, sourcePointOfSale.DeviceInfo);
            Assert.Equal(ipAddress, sourcePointOfSale.IpAddress);
            Assert.Equal(browserInfo, sourcePointOfSale.BrowserInfo);
            Assert.DoesNotContain("deviceInfo", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("ipAddress", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("browserInfo", serializedRequest, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(deviceInfo, serializedRequest, StringComparison.Ordinal);
            Assert.DoesNotContain(ipAddress, serializedRequest, StringComparison.Ordinal);
            Assert.DoesNotContain(browserInfo, serializedRequest, StringComparison.Ordinal);
        }

        [Fact]
        public void CreateInRuleRequest_MapsAllRequiredDataListFieldsFromTheUpdatedOrder()
        {
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(null, updatedOrderDocument, "correlation-required-fields");
            var requestOrder = Assert.Single(request.Order);

            Assert.Same(updatedOrderDocument.DataList.OriginDestinationAssociations, requestOrder.DataList.OriginDestinationAssociations);
            Assert.Empty(requestOrder.DataList.CustomerDetails);
            Assert.Same(updatedOrderDocument.DataList.OrderInstructions, requestOrder.DataList.OrderInstructions);
            Assert.Same(updatedOrderDocument.DataList.SegmentDetails, requestOrder.DataList.SegmentDetails);
            Assert.Same(updatedOrderDocument.DataList.Penalty, requestOrder.DataList.Penalty);
            Assert.Same(updatedOrderDocument.DataList.PriceClass, requestOrder.DataList.PriceClass);
            Assert.Same(updatedOrderDocument.DataList.ServiceDefinitions, requestOrder.DataList.ServiceDefinitions);
            Assert.Same(updatedOrderDocument.DataList.Journey, requestOrder.DataList.Journey);
            Assert.NotNull(requestOrder.Parties);
        }

        [Fact]
        public void CreateInRuleRequest_MapsHistoricalAndCurrentOrdersWhenHistoryIsAvailable()
        {
            var originalOrderDocument = CreateOrderDocument(segmentOneChanged: false, segmentTwoChanged: false);
            originalOrderDocument.DocType = "Indirect";
            originalOrderDocument.FareTypeRequest = new FareTypeRequest { FareTypeCode = "70J" };
            originalOrderDocument.OrderStatus = "Historical";
            originalOrderDocument.Version = 1;
            originalOrderDocument.IsPostSaleSSR = true;

            var historicalDataList = new OrderMsDataList
            {
                SegmentDetails = [CreateSegment("SEG1", isScheduleChanged: true, statusCode: "TK")]
            };
            IEnumerable<TimeLimit> historicalTimeLimits = [new TimeLimit { Type = TimeLimit.Types.TicketingTimeLimit, Limit = "2026-08-28T13:44+01:00" }];
            var latestOrderHistory = new VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderHistory
            {
                DataList = historicalDataList,
                OrderItems = originalOrderDocument.OrderItems,
                TimeLimits = historicalTimeLimits,
                OrderStatus = "ScheduleChanged",
                ChangeType = ChangeTypeEnum.FlightDateTimeChange,
                ChangeDateTime = "2026-08-26T09:14:56Z",
                Version = "1",
                IsSplit = true
            };
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            updatedOrderDocument.Version = 2;

            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(
                originalOrderDocument,
                updatedOrderDocument,
                "correlation-history-current",
                latestOrderHistory);
            var serializedRequest = Json.Deserialize<JObject>(Json.Serialize(request));

            Assert.Equal(2, request.Order.Count);
            var historicalOrder = request.Order[0];
            var currentOrder = request.Order[1];
            Assert.Equal("Indirect", historicalOrder.DocType);
            Assert.Equal("70J", historicalOrder.FareTypeRequest.FareTypeCode);
            Assert.Same(historicalDataList.SegmentDetails, historicalOrder.DataList.SegmentDetails);
            Assert.Equal(
                originalOrderDocument.OrderItems.Select(orderItem => orderItem.OrderItemId),
                historicalOrder.OrderItems.Select(orderItem => orderItem.OrderItemId));
            Assert.Same(historicalTimeLimits, historicalOrder.TimeLimits);
            Assert.Equal("ScheduleChanged", historicalOrder.OrderStatus);
            Assert.Equal("FlightDateTimeChange", historicalOrder.ChangeType);
            Assert.Equal("2026-08-26T09:14:56Z", historicalOrder.ChangeDateTime.UtcDateTime);
            Assert.Equal(1, historicalOrder.Version);
            Assert.True(historicalOrder.IsSplit);
            Assert.True(historicalOrder.IsPostSaleSSR);
            Assert.Equal(2, currentOrder.Version);
            Assert.Equal(2, serializedRequest["order"]?.Count());
            Assert.Null(serializedRequest["order"]?[0]?["_rid"]);
            Assert.DoesNotContain("paymentDetails", Json.Serialize(request), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateInRuleRequest_MapsParticipantContacts()
        {
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            updatedOrderDocument.Parties = new VAA.EAPI.NDC.Order.Infrastructure.Model.Parties
            {
                Participants =
                [
                    new VAA.EAPI.NDC.Order.Infrastructure.Model.Participant
                    {
                        PartyType = VAA.EAPI.NDC.Order.Infrastructure.Model.Participant.PartyTypeEnum.Seller,
                        Contacts = new Contacts { Email = "seller@example.com" }
                    }
                ]
            };

            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(null, updatedOrderDocument, "correlation-participant-contacts");
            var serializedRequest = Json.Deserialize<JObject>(Json.Serialize(request));

            var participant = Assert.Single(Assert.Single(request.Order).Parties.Participants);
            Assert.Equal("seller@example.com", participant.Contacts.Email);
            Assert.Equal("seller@example.com", serializedRequest["order"]?.Single()?["parties"]?["participants"]?.Single()?["contacts"]?["email"]?.Value<string>());
        }

        [Fact]
        public void CreateInRuleRequest_MapsBaggageAllowancesFromTheUpdatedOrder()
        {
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            var baggageAllowance = new BaggageAllowances { BaggageAllowanceId = "BAG1" };
            updatedOrderDocument.DataList.BaggageAllowances = [baggageAllowance];

            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(null, updatedOrderDocument, "correlation-baggage-allowances");
            var serializedRequest = Json.Deserialize<JObject>(Json.Serialize(request));

            var mappedBaggageAllowance = Assert.Single(Assert.Single(request.Order).DataList.BaggageAllowances);
            Assert.Same(baggageAllowance, mappedBaggageAllowance);
            Assert.Equal("BAG1", serializedRequest["order"]?.Single()?["dataList"]?["baggageAllowances"]?.Single()?["baggageAllowanceId"]?.Value<string>());
        }

        [Fact]
        public void MapInRuleSellerFollowUpActions_MapsResponseActionCombinationToAllOrderItems()
        {
            var originalOrderDocument = CreateOrderDocument(segmentOneChanged: false, segmentTwoChanged: false);
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: true);
            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(originalOrderDocument, updatedOrderDocument, "correlation-subset");

            OrderChangeNotifCommandHandler.MapInRuleSellerFollowUpActions(updatedOrderDocument, CreateResponse(request, "Accept, Reshop, Cancel", "SEG1"));

            Assert.Equal(["Accept", "Reshop", "Cancel"], GetOrderItem(updatedOrderDocument, "SEG1").SellerFollowUpAction);
            Assert.Equal(["Accept", "Reshop", "Cancel"], GetOrderItem(updatedOrderDocument, "SEG2").SellerFollowUpAction);
        }

        [Fact]
        public void IsValidInRuleResponse_DoesNotRequireOrderDetails()
        {
            var originalOrderDocument = CreateOrderDocument(segmentOneChanged: false, segmentTwoChanged: false);
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: true);
            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(originalOrderDocument, updatedOrderDocument, "correlation-multiple-actions");

            var response = CreateResponse(request, "Accept");
            response.OrderDetails = null;

            Assert.True(OrderChangeNotifCommandHandler.IsValidInRuleResponse(updatedOrderDocument, request, response));
        }

        [Theory]
        [InlineData("Accept")]
        [InlineData("Reshop")]
        [InlineData("Cancel")]
        [InlineData("Accept,Reshop")]
        [InlineData("Accept,Reshop,Cancel")]
        public void IsValidInRuleResponse_ReturnsTrueForSellerFollowUpActionEnumCombinations(string outcome)
        {
            var originalOrderDocument = CreateOrderDocument(segmentOneChanged: false, segmentTwoChanged: false);
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(originalOrderDocument, updatedOrderDocument, "correlation-actions");

            Assert.True(OrderChangeNotifCommandHandler.IsValidInRuleResponse(
                updatedOrderDocument,
                request,
                CreateResponse(request, outcome)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("Unsupported")]
        [InlineData("Accept,Unsupported")]
        [InlineData("Accept,Accept")]
        public void IsValidInRuleResponse_ReturnsFalseForInvalidSellerFollowUpActions(string outcome)
        {
            var originalOrderDocument = CreateOrderDocument(segmentOneChanged: false, segmentTwoChanged: false);
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(originalOrderDocument, updatedOrderDocument, "correlation-no-actions");
            var response = CreateResponse(request, outcome, "SEG1");

            Assert.False(OrderChangeNotifCommandHandler.IsValidInRuleResponse(updatedOrderDocument, request, response));
            OrderChangeNotifCommandHandler.MapInRuleSellerFollowUpActions(updatedOrderDocument, response);

            Assert.Null(GetOrderItem(updatedOrderDocument, "SEG1").SellerFollowUpAction);
            Assert.Null(GetOrderItem(updatedOrderDocument, "SEG2").SellerFollowUpAction);
        }

        [Theory]
        [InlineData("UNKNOWN", "SEG1")]
        [InlineData("SEG1", "SEG1")]
        public void IsValidInRuleResponse_IgnoresReturnedOrderDetails(string firstSegmentId, string secondSegmentId)
        {
            var originalOrderDocument = CreateOrderDocument(segmentOneChanged: false, segmentTwoChanged: false);
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(originalOrderDocument, updatedOrderDocument, "correlation-invalid");

            var response = CreateResponse(request, "Accept", firstSegmentId, secondSegmentId);

            Assert.True(OrderChangeNotifCommandHandler.IsValidInRuleResponse(updatedOrderDocument, request, response));
        }

        [Fact]
        public void IsValidInRuleResponse_ReturnsFalseWhenEventVersionDiffers()
        {
            var originalOrderDocument = CreateOrderDocument(segmentOneChanged: false, segmentTwoChanged: false);
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            var request = OrderChangeNotifCommandHandler.CreateInRuleRequest(originalOrderDocument, updatedOrderDocument, "correlation-event-version");
            var response = CreateResponse(request, "Accept", "SEG1");
            response.Event = new Event
            {
                EventId = request.Event.EventId,
                EventType = request.Event.EventType,
                EventVersion = "2",
                Source = request.Event.Source,
                OccurredAt = request.Event.OccurredAt,
                CorrelationId = request.Event.CorrelationId
            };

            Assert.False(OrderChangeNotifCommandHandler.IsValidInRuleResponse(updatedOrderDocument, request, response));
        }

        [Fact]
        public async Task ApplyInRuleAdviceAsync_UsesSynchronizedOrderDocumentsAndMapsMatchedActions()
        {
            using var inRuleConfiguration = ConfigureMock();
            using var serviceProvider = CreateServiceProvider();
            var responsesBefore = _httpClientFactoryFixture.Current.GetResponses(MockRequestUrl).Count;
            var commandHandler = CreateCommandHandler(serviceProvider);
            var originalOrderDocument = CreateOrderDocument(segmentOneChanged: false, segmentTwoChanged: false);
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            var asyncContext = new AsyncContext();
            asyncContext.SetCorrelationId(Guid.Parse(SuccessCorrelationId));
            AsyncContext.Current = asyncContext;

            try
            {
                await commandHandler.ApplyInRuleAdviceAsync(
                    new VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderHistory
                    {
                        ChangeType = ChangeTypeEnum.FlightDateTimeChange,
                        ReasonCode = Constants.Application.InvoluntaryScheduleChangeReasonCode
                    },
                    originalOrderDocument,
                    updatedOrderDocument,
                    CancellationToken.None);

                var responses = _httpClientFactoryFixture.Current.GetResponses(MockRequestUrl);
                Assert.Equal(responsesBefore + 1, responses.Count);
                Assert.Equal(["Accept"], GetOrderItem(updatedOrderDocument, "SEG1").SellerFollowUpAction);
                Assert.Equal(["Accept"], GetOrderItem(updatedOrderDocument, "SEG2").SellerFollowUpAction);
            }
            finally
            {
                AsyncContext.Current = null;
            }
        }

        [Fact]
        public async Task ApplyInRuleAdviceAsync_ContinuesWithoutActionsWhenInRuleTimesOut()
        {
            using var inRuleConfiguration = ConfigureMock();
            using var serviceProvider = CreateServiceProvider(TimeSpan.FromMilliseconds(25));
            var commandHandler = CreateCommandHandler(serviceProvider);
            var originalOrderDocument = CreateOrderDocument(segmentOneChanged: false, segmentTwoChanged: false);
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);
            var asyncContext = new AsyncContext();
            asyncContext.SetCorrelationId(Guid.Parse(TimeoutCorrelationId));
            AsyncContext.Current = asyncContext;

            try
            {
                await commandHandler.ApplyInRuleAdviceAsync(
                    new VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderHistory
                    {
                        ChangeType = ChangeTypeEnum.FlightDateTimeChange,
                        ReasonCode = Constants.Application.InvoluntaryScheduleChangeReasonCode
                    },
                    originalOrderDocument,
                    updatedOrderDocument,
                    CancellationToken.None);

                Assert.Null(GetOrderItem(updatedOrderDocument, "SEG1").SellerFollowUpAction);
                Assert.Null(GetOrderItem(updatedOrderDocument, "SEG2").SellerFollowUpAction);
            }
            finally
            {
                AsyncContext.Current = null;
            }
        }

        [Fact]
        public async Task ApplyInRuleAdviceAsync_RethrowsWhenCallerCancellationIsRequested()
        {
            using var inRuleConfiguration = ConfigureMock();
            using var serviceProvider = CreateServiceProvider();
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var commandHandler = CreateCommandHandler(serviceProvider);
            var originalOrderDocument = CreateOrderDocument(segmentOneChanged: false, segmentTwoChanged: false);
            var updatedOrderDocument = CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false);

            await Assert.ThrowsAsync<OperationCanceledException>(() => commandHandler.ApplyInRuleAdviceAsync(
                new VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderHistory
                {
                    ChangeType = ChangeTypeEnum.FlightDateTimeChange,
                    ReasonCode = Constants.Application.InvoluntaryScheduleChangeReasonCode
                },
                originalOrderDocument,
                updatedOrderDocument,
                cancellationTokenSource.Token));
        }

        [Theory]
        [InlineData(ChangeTypeEnum.FlightDateTimeChange, true)]
        [InlineData(ChangeTypeEnum.FlightCancellation, true)]
        [InlineData(ChangeTypeEnum.FlightNumberChange, false)]
        [InlineData(ChangeTypeEnum.FlightRouteChange, false)]
        public void ShouldInvokeInRule_OnlyReturnsTrueForSupportedDisruptionChangeTypesRegardlessOfReasonCode(
            ChangeTypeEnum changeType,
            bool expectedResult)
        {
            var shouldInvoke = OrderChangeNotifCommandHandler.ShouldInvokeInRule(
                new VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderHistory
                {
                    ChangeType = changeType,
                    ReasonCode = "111"
                },
                CreateOrderDocument(segmentOneChanged: true, segmentTwoChanged: false));

            Assert.Equal(expectedResult, shouldInvoke);
        }

        private static DisruptionResponse CreateResponse(DisruptionRequest request, string value, params string[] segmentIds) =>
            new()
            {
                Event = request.Event,
                InRuleResponse = new ResponseDetails
                {
                    Value = value,
                    RuleId = "R5_SCH_Major",
                    CorrelationId = request.CorrelationId
                },
                OrderDetails = segmentIds.Select(segmentId => new OrderDetails
                {
                    OrderId = request.OrderId,
                    Rloc = request.BookingReference,
                    SegmentId = segmentId
                }).ToList()
            };

        private static OrderDocument CreateOrderDocument(bool segmentOneChanged, bool segmentTwoChanged) =>
            new()
            {
                Id = "VS932OWWGYG26",
                Version = 2,
                OrderNumber = new OrderNumber { OrderId = "VS932OWWGYG26" },
                BookingReference = new BookingReference { RLOC = "OWWGYG" },
                PointOfSale = new Pos(),
                CreationDateTime = new UtcAndLocalDateTime { UtcDateTime = "2027-05-26T18:10:00Z" },
                ChangeDateTime = new UtcAndLocalDateTime { UtcDateTime = "2027-05-26T18:10:00Z" },
                DataList = new OrderMsDataList
                {
                    OriginDestinationAssociations = [],
                    CustomerDetails = [],
                    OrderInstructions = [],
                    SegmentDetails = [CreateSegment("SEG1", segmentOneChanged, "TK"), CreateSegment("SEG2", segmentTwoChanged, "HK")],
                    Penalty = [],
                    PriceClass = [],
                    ServiceDefinitions = [],
                    Journey = []
                },
                OrderItems = [CreateOrderItem("ITEM1", "SEG1"), CreateOrderItem("ITEM2", "SEG2")],
                Parties = new VAA.EAPI.NDC.Order.Infrastructure.Model.Parties(),
                ChangeType = ChangeTypeEnum.OrderItemAdded
            };

        private static Segment CreateSegment(string segmentId, bool isScheduleChanged, string statusCode)
        {
            var departureDateTime = isScheduleChanged ? "2027-05-26T22:10:00" : "2027-05-26T18:10:00";
            var arrivalDateTime = isScheduleChanged ? "2027-05-27T01:10:00" : "2027-05-26T21:10:00";

            return new Segment
            {
                SegmentId = segmentId,
                OriginAirportCode = "LHR",
                DestinationAirportCode = "SFO",
                MarketingCarrier = "VS",
                MarketingFlightNumber = segmentId == "SEG1" ? "19" : "20",
                ScheduledDepartureTime = new UtcAndLocalDateTime { LocalDateTime = departureDateTime },
                ScheduledArrivalTime = new UtcAndLocalDateTime { LocalDateTime = arrivalDateTime },
                Status = new Status { Code = statusCode },
                ScheduleChangeIndicator = isScheduleChanged
            };
        }

        private static OrderItems CreateOrderItem(string orderItemId, string segmentId) =>
            new()
            {
                OrderItemId = orderItemId,
                Services = [new OrderService
                {
                    SegmentRefIds = [segmentId],
                    ServiceBeneficiaries = [segmentId == "SEG1" ? "PAX1" : "PAX2"],
                    Product = new Product { ProductType = Product.ProductTypes.FlightEnum }
                }],
                AccountableDocumentRefs = [new AccountableDocumentRef
                {
                    Coupons = [new Coupon { SegmentRefId = segmentId, CouponStatus = "OPEN" }]
                }]
            };

        private static OrderItems GetOrderItem(OrderDocument orderDocument, string segmentId) =>
            orderDocument.OrderItems.Single(orderItem => orderItem.Services.Any(service => service.SegmentRefIds.Contains(segmentId)));

        private ServiceProvider CreateServiceProvider(TimeSpan? lookupTimeout = null)
        {
            var services = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(_httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider, TestOAuthTokenProvider>()
                .AddSingleton<IOrderNotificationMSCommand, OrderNotificationMSCommandHandler>()
                .AddSingleton<IBrandMSCommand, BrandMSCommandHandler>()
                .AddSingleton<IBrandMSPTCCommand, BrandMSCommandHandlerPTC>()
                .AddSingleton<IMessageBusFactory, AzureServiceBusFactory>()
                .AddSingleton<IServiceBusTopicMessageEventCommandHandler, ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<ITrackAnalyticsEventCommandHandler, TrackAnalyticsEventCommandHandler>();

            if (lookupTimeout.HasValue)
            {
                services.AddSingleton<InRuleService>(serviceProvider => new InRuleService(
                    serviceProvider.GetRequiredService<IHttpConnector>(),
                    serviceProvider.GetRequiredService<IOAuthTokenProvider>(),
                    serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<InRuleService>>(),
                    lookupTimeout.Value));
            }
            else
            {
                services.AddSingleton<InRuleService>();
            }

            services.AddSingleton<IInRuleService>(serviceProvider => serviceProvider.GetRequiredService<InRuleService>());

            return services.BuildServiceProvider();
        }

        private static InRuleConfigurationScope ConfigureMock() => InRuleConfigurationScope.Create(InRuleUrl, "api://inrule", "test-inrule-subscription-key");

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("test-oauth-token");
        }

        private static OrderChangeNotifCommandHandler CreateCommandHandler(IServiceProvider serviceProvider) =>
            new(
                NullLogger<OrderChangeNotifCommandHandler>.Instance,
                serviceProvider.GetRequiredService<IOrderNotificationMSCommand>(),
                null,
                null,
                null,
                null,
                null,
                serviceProvider.GetRequiredService<IBrandMSCommand>(),
                serviceProvider.GetRequiredService<IBrandMSPTCCommand>(),
                serviceProvider.GetRequiredService<IInRuleService>(),
                serviceProvider.GetRequiredService<ITrackAnalyticsEventCommandHandler>());
    }
}
