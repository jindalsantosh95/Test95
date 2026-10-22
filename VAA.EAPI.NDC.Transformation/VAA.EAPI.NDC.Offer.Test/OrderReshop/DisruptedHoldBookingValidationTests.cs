using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.ReshopFareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Validations.OrderReshop;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshop
{
    public class DisruptedHoldBookingValidationTests
    {
        private readonly IHostEnvironment _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };

        [Fact]
        public void DisruptedHoldBooking_DoesNotApplyStandardReshopLimit()
        {
            var orderDocument = GetHoldBookingDocument();
            orderDocument.ReshopHoldBookingCounter = 6;
            orderDocument.DataList.SegmentDetails.Single(x => x.OdRefId == "OD2").ScheduleChangeIndicator = true;

            var response = Validate(orderDocument, isHoldBookingReshop: true, isReshopDisruption: true);

            Assert.Null(response);
        }

        [Fact]
        public void StandardHoldBooking_AtReshopLimit_ReturnsLimitError()
        {
            var orderDocument = GetHoldBookingDocument();
            orderDocument.ReshopHoldBookingCounter = 6;

            var response = Validate(orderDocument, isHoldBookingReshop: true, isReshopDisruption: false);

            Assert.NotNull(response);
            Assert.Contains(response.ResponseMessages?.Errors ?? [], error => error.Id == "790_2");
        }

        [Fact]
        public void TicketedDisruption_ValidatesFlownSegment()
        {
            var orderDocument = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\OrderDoc_MultiPax");
            orderDocument.DataList.SegmentDetails.Single(x => x.OdRefId == "OD2").ScheduleChangeIndicator = true;

            foreach (var coupon in orderDocument.OrderItems.SelectMany(orderItem => orderItem.AccountableDocumentRefs).SelectMany(accountableDocument => accountableDocument.Coupons))
            {
                coupon.CouponStatus = "USED";
            }

            var response = Validate(orderDocument, isHoldBookingReshop: false, isReshopDisruption: true);

            Assert.NotNull(response);
            Assert.Contains(response.ResponseMessages?.Errors ?? [], error => error.Id == "790_1");
        }

        [Fact]
        public void TicketedDisruption_AtTicketValidityEndDate_IsValid()
        {
            var orderDocument = GetTicketedDisruptionDocument();

            var errors = ValidateOrderReshop.ValidateDisruptionScopeAndTicketIssueDate(GetValidReshopRequest("2027-01-01"), orderDocument, 365);

            Assert.Empty(errors);
        }

        [Fact]
        public void TicketedDisruption_AfterConfiguredTicketValidityEndDate_ReturnsDateError()
        {
            var orderDocument = GetTicketedDisruptionDocument();

            var errors = ValidateOrderReshop.ValidateDisruptionScopeAndTicketIssueDate(GetValidReshopRequest("2026-02-01"), orderDocument, 30);

            Assert.Contains(errors, error => error.ErrorCode == "16");
        }

        [Fact]
        public void NonTicketedDisruption_DoesNotApplyTicketValidityWindow()
        {
            var orderDocument = GetHoldBookingDocument();
            orderDocument.DataList.SegmentDetails.Single(x => x.OdRefId == "OD2").ScheduleChangeIndicator = true;

            var errors = ValidateOrderReshop.ValidateDisruptionScopeAndTicketIssueDate(GetValidReshopRequest("2030-01-01"), orderDocument, 30);

            Assert.Empty(errors);
        }

        private Response<ErrorType> Validate(OrderDocument orderDocument, bool isHoldBookingReshop, bool isReshopDisruption) => OrderReshopRequestValidations.ValidateOrderReshopRequestAgainstDocument<ErrorType>(isHoldBookingReshop, GetValidReshopRequest(), orderDocument, GetReshopFareFamilies(), null, _hostEnvironment, isReshopDisruption, ticketValidityWindowInDays: 365);

        private static OrderDocument GetTicketedDisruptionDocument()
        {
            var orderDocument = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\OrderDoc_MultiPax");
            orderDocument.DataList.SegmentDetails.Single(x => x.OdRefId == "OD2").ScheduleChangeIndicator = true;

            foreach (var documentReference in orderDocument.OrderItems.SelectMany(orderItem => orderItem.AccountableDocumentRefs))
            {
                documentReference.IssueDate = "2026-01-01";
            }

            return orderDocument;
        }

        [Fact]
        public void TicketedDisruption_AllowsReshopForANonDisruptedPnrSegment()
        {
            var orderDocument = GetTicketedDisruptionDocument();
            var request = GetValidReshopRequest();
            request.OriginDestinationAssociations.Single().OriginDestination = new()
            {
                OriginAirport = "LHR",
                DestinationAirport = "ATL",
                DepartureDate = "2026-07-01"
            };

            var response = OrderReshopRequestValidations.ValidateOrderReshopRequestAgainstDocument<ErrorType>(
                false, request, orderDocument, GetReshopFareFamilies(), null, _hostEnvironment, true, ticketValidityWindowInDays: 365);

            Assert.Null(response);
        }

        [Fact]
        public void TicketedDisruption_AllowsReshopForEveryPnrSegment()
        {
            var orderDocument = GetTicketedDisruptionDocument();
            var request = GetFullJourneyReshopRequest();

            var response = OrderReshopRequestValidations.ValidateOrderReshopRequestAgainstDocument<ErrorType>(
                false, request, orderDocument, GetReshopFareFamilies(), null, _hostEnvironment, true, ticketValidityWindowInDays: 365);

            Assert.Null(response);
        }

        [Fact]
        public void TicketedDisruption_DoesNotApplyTicketValidityWindowToANonDisruptedPnrSegment()
        {
            var orderDocument = GetTicketedDisruptionDocument();
            var request = GetValidReshopRequest("2030-01-01");
            request.OriginDestinationAssociations.Single().OriginDestination = new()
            {
                OriginAirport = "LHR",
                DestinationAirport = "ATL",
                DepartureDate = "2030-01-01"
            };

            var errors = ValidateOrderReshop.ValidateDisruptionScopeAndTicketIssueDate(request, orderDocument, 30);

            Assert.Empty(errors);
        }

        [Fact]
        public void TicketedDisruption_UsesEachDisruptedSegmentTicketForItsValidityWindow()
        {
            var orderDocument = GetTicketedDisruptionDocument();
            foreach (var segment in orderDocument.DataList.SegmentDetails)
            {
                segment.ScheduleChangeIndicator = true;
            }

            var ticketDocuments = orderDocument.OrderItems
                .SelectMany(orderItem => orderItem.AccountableDocumentRefs)
                .ToList();
            ticketDocuments[0].Coupons = ticketDocuments[0].Coupons
                .Where(coupon => coupon.SegmentRefId == "SEG1")
                .ToArray();
            ticketDocuments[0].IssueDate = "2026-01-01";
            foreach (var ticketDocument in ticketDocuments.Skip(1))
            {
                ticketDocument.Coupons = ticketDocument.Coupons
                    .Where(coupon => coupon.SegmentRefId == "SEG2")
                    .ToArray();
                ticketDocument.IssueDate = "2025-01-01";
            }

            var request = GetFullJourneyReshopRequest();

            var errors = ValidateOrderReshop.ValidateDisruptionScopeAndTicketIssueDate(request, orderDocument, 365);

            Assert.Contains(errors, error => error.ErrorCode == "16");
        }

        [Fact]
        public void TicketedDisruption_RejectsAnOriginDestinationThatIsNotInThePnr()
        {
            var orderDocument = GetTicketedDisruptionDocument();
            var request = GetValidReshopRequest();
            request.OriginDestinationAssociations.Single().OriginDestination = new()
            {
                OriginAirport = "LHR",
                DestinationAirport = "JFK",
                DepartureDate = "2026-07-01"
            };

            var errors = ValidateOrderReshop.ValidateDisruptionScopeAndTicketIssueDate(request, orderDocument, 365);

            Assert.Contains(errors, error => error.ErrorCode == "129_5");
        }

        private static OrderDocument GetHoldBookingDocument()
        {
            var orderDocument = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\OrderDoc_MultiPax");
            orderDocument.TimeLimits = [new()];

            foreach (var orderItem in orderDocument.OrderItems)
            {
                orderItem.AccountableDocumentRefs = null;
            }

            return orderDocument;
        }

        private static OrderReshopRequest GetValidReshopRequest(string departureDate = "2026-07-08")
        {
            var orderRequest = TestDataProvider.GetTestData<OrderReshopRequest>("NDCOffer\\OrderReshop\\ReshopRQMultiPax");
            orderRequest.OriginDestinationAssociations.Single().OriginDestination.DepartureDate = departureDate;

            foreach (var reshopOrder in orderRequest.ReshopOrder)
            {
                reshopOrder.Services = [new() { ServiceId = "SV1" }, new() { ServiceId = "SV3" }];
            }

            return orderRequest;
        }

        private static OrderReshopRequest GetFullJourneyReshopRequest()
        {
            var orderRequest = GetValidReshopRequest();
            orderRequest.ReshopOrder = [];
            orderRequest.OriginDestinationAssociations =
            [
                new() { OriginDestination = new() { OriginAirport = "LHR", DestinationAirport = "ATL", DepartureDate = "2026-07-01" } },
                new() { OriginDestination = new() { OriginAirport = "ATL", DestinationAirport = "LHR", DepartureDate = "2026-07-08" } }
            ];

            return orderRequest;
        }

        private static IEnumerable<ReshopFareFamily> GetReshopFareFamilies() => [new() { CabinCode = "5", ExistingFareFamily = "Economy Delight", ItaFareFamilies = ["Y"] }];
    }
}
