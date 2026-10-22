using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Mapper.AirShopping;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.AirShopping.Mapper
{
    public class MapOfferResponseTests
    {
        private readonly Mock<IGenericDataMapper> _genericDataMapper;
        private readonly IEnumerable<NDCWarning> warningList;

        public MapOfferResponseTests()
        {
            _genericDataMapper = new Mock<IGenericDataMapper>();
            warningList = Enumerable.Empty<NDCWarning>();
        }

        [Fact]
        public void Should_Return_SUCCESS_For_Multicity_MapOfferResponse()
        {
            var offerResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\Airshopping\\2ADT1CHD1GBEMulticity");
            var calendarSearchofferResponse = TestDataProvider.GetTestData<CalendarSearchResponse>("NDCOffer\\Airshopping\\CalendarSearchResponse");
            string fareTypeCode = Constants.Application.PrivateFareTypeCode;
            IEnumerable<string> fareSubTypeCode = Constants.Application.FareSubTypeCodes;
            var iATA_AirShoppingRS = AirshoppingResponseMapper.MapOfferResponse(offerResponse, _genericDataMapper.Object, null, warningList, fareSubTypeCode.First(), fareTypeCode);

            Assert.NotNull(iATA_AirShoppingRS);
            Assert.Equal("SEG1", (((AirShoppingResponseType[])iATA_AirShoppingRS.Items)[0]).DataLists.PaxSegmentList[0].PaxSegmentID);
            Assert.Equal("H", (((AirShoppingResponseType[])iATA_AirShoppingRS.Items)[0]).DataLists.PaxSegmentList[0].MarketingCarrierRBD_Code);
            Assert.Equal("IAD", (((AirShoppingResponseType[])iATA_AirShoppingRS.Items)[0]).DataLists.OriginDestList[0].OriginCode);
            Assert.Equal("LHR", (((AirShoppingResponseType[])iATA_AirShoppingRS.Items)[0]).DataLists.OriginDestList[0].DestCode);
            Assert.Equal("758", (((AirShoppingResponseType[])iATA_AirShoppingRS.Items)[0]).OffersGroup.CarrierOffers[0].Offer.First().OfferItem.First().FareDetail.First().FareComponent.First().NegotiatedCode);
            Assert.Equal("SEAT ONLY", (((AirShoppingResponseType[])iATA_AirShoppingRS.Items)[0]).OffersGroup.CarrierOffers[0].Offer.First().OfferItem.First().FareDetail.First().FareComponent.First().FareTypeCode);
        }

    }
}
