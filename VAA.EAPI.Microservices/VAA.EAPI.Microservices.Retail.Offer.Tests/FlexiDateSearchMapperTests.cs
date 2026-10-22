using System.Linq;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlexiDateSearch;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class FlexiDateSearchMapperTests
    {
        [Fact]
        public void FlexiSearchRQMapperTest()
        {
            //Arrange 
            var request = TestDataProvider.GetTestData<FlexiDateOfferRequest>("OfferMS\\OfferMS_FlexiDateSearchRQ");

            //Act
            var deltaRQ = new FlexiDateSearchQuery(request);

            //Assert
            Assert.True(deltaRQ.Request.Customers.Count() == 1);
            Assert.True(deltaRQ.Request.OffersCriteria.FlightRequestCriteria.SearchOriginDestination.Count() == 2);
            Assert.NotNull(deltaRQ.Request.OffersCriteria.FlightRequestCriteria.SearchOriginDestination.First().DepartureLocalTs);
            Assert.NotNull(deltaRQ.Request.Customers.First().CustomerId);
            Assert.NotNull(deltaRQ.Request.OffersCriteria.FlightRequestCriteria.SearchOriginDestination.First().Origins.First().AirportCode);
            Assert.NotEmpty(deltaRQ.Request.Customers.First().PassengerTypeCode);
            Assert.NotNull(deltaRQ.Request.OffersCriteria.FlightRequestCriteria.SearchOriginDestination.First().CalenderDateRequest);
            Assert.True(deltaRQ.Request.OffersCriteria.FlightRequestCriteria.SearchOriginDestination.Last().CalenderDateRequest.DaysBeforeCnt == 3);
        }

        //[Fact]
        //public void FlexiSearchRSMapperTest()
        //{
        //    //Arrange 
        //    var response = TestDataProvider.GetTestData<OffersSetList>("OfferMS\\DeltaOffer_RS");
        //    var request = TestDataProvider.GetTestData<FlexiSearchQuery>("OfferMS\\OfferMS_FlexiDateSearchRQ");

        //    //Act
        //    var offerMicroRs = FlexiSearchResMapper.FlexiDateSearchResMapper(response, request);

        //    //Assert
        //    Assert.True(offerMicroRs.ShoppingResponseId == "5db608bd-f0f2-43e7-b053-e06c7d419ba2");
        //    Assert.True(offerMicroRs.FlexibleDateCalendar.ColumnText.Count() == 7);
        //    Assert.NotNull(offerMicroRs.FlexibleDateCalendar.DepartureDetails.First().FlexibleDatePriceCalendarCell.First().OriginDestinations);
        //    Assert.True(offerMicroRs.FlexibleDateCalendar.DepartureDetails.Count() == 7);
        //    Assert.True(offerMicroRs.FlexibleDateCalendar.DepartureDetails.First().FlexibleDatePriceCalendarCell.Count() == 7);
        //    Assert.True(offerMicroRs.FlexibleDateCalendar.DepartureDetails.First().FlexibleDatePriceCalendarCell.First().OriginDestinations.Count() == 2);
        //    Assert.NotNull(offerMicroRs.FlexibleDateCalendar.DepartureDetails.First().FlexibleDatePriceCalendarCell.First().OfferId);
        //}
    }
}
