using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Application.OrderRetrieve;
using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderRetrieveRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper.ErrorMapper;
using VAA.EAPI.NDC.Order.Mapper.Order;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderRetrieve
{
    public class BrandMSRequestMapperTests
    {

        [Fact]
        public void BrandMSRequesMapper_Test()
        {

            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDoc");
            var brandMSresp = BrandMSRequestMapper.MapBrandMSOrderRequest(orderDoc);

            var brandMsegementdetails = brandMSresp.Itinerary.First().Segments;
            var segmentList = orderDoc.DataList.SegmentDetails.ToList();

            Assert.Equal(segmentList.Count(), brandMsegementdetails.Count());

            foreach (var seg in segmentList)
            {
                Assert.Contains(brandMsegementdetails, x => x.FlightNumber == Convert.ToInt16(orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg.SegmentId)?.MarketingFlightNumber));
                Assert.Equal(brandMSresp.Currency, orderDoc.PointOfSale.CurrencyCode);
                Assert.Contains(brandMsegementdetails, x => x.Carrier == orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg.SegmentId)?.MarketingCarrier);
                Assert.Contains(brandMsegementdetails, x => x.DepartureDate == Convert.ToDateTime(orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg.SegmentId)?.ScheduledDepartureTime.LocalDateTime));
                Assert.Contains(brandMsegementdetails, x => x.Origin == orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg.SegmentId)?.OriginAirportCode);
                Assert.Contains(brandMsegementdetails, x => x.Destination == orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg.SegmentId)?.DestinationAirportCode);

            }

        }

        [Theory]
        [InlineData("758")]
        [InlineData("70J")]
        public void BrandMSRequestMapper_IncludesTicketDesignator_ForSupportedFare(string fareTypeCode)
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDoc");
            orderDoc.FareTypeRequest = new() { FareTypeCode = fareTypeCode };
            foreach (var priceClass in orderDoc.DataList.PriceClass)
            {
                priceClass.TicketDesigCode = "UWF1";
            }

            var result = BrandMSRequestMapper.MapBrandMSOrderRequest(orderDoc);

            Assert.Equal("UWF1", result.Itinerary.First().Segments.First().TicketDesignator);
        }

        [Fact]
        public void BrandMSRequestMapper_DoesNotIncludeTicketDesignator_ForCorporateFare()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDoc");
            orderDoc.FareTypeRequest = new() { FareTypeCode = Constants.Application.CorporateFareTypeCode };
            foreach (var priceClass in orderDoc.DataList.PriceClass)
            {
                priceClass.TicketDesigCode = "UWF1";
            }

            var result = BrandMSRequestMapper.MapBrandMSOrderRequest(orderDoc);

            Assert.Null(result.Itinerary.First().Segments.First().TicketDesignator);
        }

        [Theory]
        [InlineData("758", true)]
        [InlineData("70J", true)]
        [InlineData("70G", false)]
        public void BrandMSRequestMapper_MapsTicketDesignator_ForInfantPtc(string fareTypeCode, bool shouldIncludeTicketDesignator)
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDoc");
            orderDoc.FareTypeRequest = new() { FareTypeCode = fareTypeCode };
            orderDoc.DataList.CustomerDetails.First().ItaPtc = Constants.Application.InfantPTCCode;
            foreach (var priceClass in orderDoc.DataList.PriceClass)
            {
                priceClass.TicketDesigCode = "UWF1";
            }

            var result = BrandMSRequestMapper.MapBrandMSPTCOrderRequest(orderDoc);

            Assert.Equal(shouldIncludeTicketDesignator ? "UWF1" : null, result.Itinerary.First().Segments.First().TicketDesignator);
        }

        [Fact]
        public void BrandMSRequesMapper_for4PAX_Oneway_Test()
        {

            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDoc_4PAX_oneway");
            var brandMSresp = BrandMSRequestMapper.MapBrandMSOrderRequest(orderDoc);

            var brandMsegementdetails = brandMSresp.Itinerary.First().Segments;
            var segmentList = orderDoc.DataList.SegmentDetails.ToList();
           
            Assert.Equal(segmentList.Count(), brandMsegementdetails.Count());

            foreach (var seg in segmentList)
            {
                Assert.Contains(brandMsegementdetails, x => x.FlightNumber == Convert.ToInt16(orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg.SegmentId)?.MarketingFlightNumber));
                Assert.Equal(brandMSresp.Currency, orderDoc.PointOfSale.CurrencyCode);
                Assert.Contains(brandMsegementdetails, x => x.Carrier == orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg.SegmentId)?.MarketingCarrier);
                Assert.Contains(brandMsegementdetails, x => x.DepartureDate == Convert.ToDateTime(orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg.SegmentId)?.ScheduledDepartureTime.LocalDateTime));
                Assert.Contains(brandMsegementdetails, x => x.Origin == orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg.SegmentId)?.OriginAirportCode);
                Assert.Contains(brandMsegementdetails, x => x.Destination == orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg.SegmentId)?.DestinationAirportCode);

            }

        }
        [Fact]
        public void BrandMSRequesMapper_for5PAX_Oneway_Test()
        {

            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDoc_5PAX_oneway");
            var brandMSresp = BrandMSRequestMapper.MapBrandMSOrderRequest(orderDoc);

            var brandMsegementdetails = brandMSresp.Itinerary.First().Segments;
            var services = orderDoc.OrderItems.First().Services.First();

            Assert.Equal(services.SegmentRefIds.Count(), brandMsegementdetails.Count());

            foreach (var seg in services.SegmentRefIds)
            {
                Assert.Contains(brandMsegementdetails, x => x.FlightNumber == Convert.ToInt16(orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg)?.MarketingFlightNumber));
                Assert.Equal(brandMSresp.Currency, orderDoc.PointOfSale.CurrencyCode);
                Assert.Contains(brandMsegementdetails, x => x.Carrier == orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg)?.MarketingCarrier);
                Assert.Contains(brandMsegementdetails, x => x.DepartureDate == Convert.ToDateTime(orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg)?.ScheduledDepartureTime.LocalDateTime));
                Assert.Contains(brandMsegementdetails, x => x.Origin == orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg)?.OriginAirportCode);
                Assert.Contains(brandMsegementdetails, x => x.Destination == orderDoc.DataList.SegmentDetails.FirstOrDefault(x => x.SegmentId == seg)?.DestinationAirportCode);

            }
        }

        [Fact]
        public void validateOrderDocVsSabreDoc_Test()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocPriceClass");
            var sabreOrderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\SabreDoc");
            var newOrdeDoc= BrandMSRequestMapper.UpdateFares(orderDoc, sabreOrderDoc);

            Assert.Equal(newOrdeDoc.DataList.PriceClass.First().FareBasisCode, orderDoc.DataList.PriceClass.First().FareBasisCode);
            Assert.Equal(newOrdeDoc.OrderItems.First().PriceDetails.Count(), orderDoc.OrderItems.First().PriceDetails.Count());
            Assert.Equal(newOrdeDoc.DataList.PriceClass.First().ProductName, orderDoc.DataList.PriceClass.First().ProductName);
            Assert.Equal(newOrdeDoc.PointOfSale.CurrencyCode, orderDoc.PointOfSale.CurrencyCode);
        }

        [Fact]
        public void UpdateFares_WhenPersistedReplacementHasOriginalSalePrice_CopiesItToSabreOrderItem()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocPriceClass");
            var sabreOrderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\SabreDoc");
            var expectedOriginalSalePrice = orderDoc.OrderItems.First().PriceDetails;
            var expectedAmounts = expectedOriginalSalePrice.Select(price => price.Amount.MonetaryAmount).ToArray();
            orderDoc.OrderItems.First().OriginalSalePrice = expectedOriginalSalePrice;
            sabreOrderDoc.OrderItems.First().OriginalSalePrice = null;

            var result = BrandMSRequestMapper.UpdateFares(orderDoc, sabreOrderDoc);

            Assert.Equal(expectedAmounts, result.OrderItems.First().OriginalSalePrice.Select(price => price.Amount.MonetaryAmount));
        }
    }
}
