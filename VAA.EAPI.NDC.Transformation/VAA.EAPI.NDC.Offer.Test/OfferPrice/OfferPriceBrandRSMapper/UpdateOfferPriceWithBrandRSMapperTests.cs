using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Mapper.BrandMs;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice.OfferPriceBrandRSMapper
{
    public class UpdateOfferPriceWithBrandRSMapperTests
    {
        [Fact]
        public void UpdateOfferPrice_ShouldUpdateKeyOfferPenaltyInfo_ChangeAndCancellRestrictions()
        {
            //Arrange
            int penaltyIndex = 1;
            int checkedBagCounter = 1;
            int serviceCounter = 1;
            IEnumerable<BaggageAllowances> baggageList = null;

            var brandMSResponseValue = TestDataProvider.GetTestData<BrandRS>("NDCOffer\\OfferPrice\\OfferPriceBrandMS\\BrandMSResponseValue");
            var offerDetailKey = TestDataProvider.GetTestData<OfferDetails>("NDCOffer\\OfferPrice\\OfferPriceBrandMS\\OfferDetailKey");
            var offerResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferPriceBrandMS\\OfferResponse");

            //Act
            var resultingOfferResponse = BrandMsRSMapper.UpdateOfferPrice(brandMSResponseValue, offerDetailKey, offerResponse, ref penaltyIndex, ref checkedBagCounter, ref serviceCounter, ref baggageList);

            var updatedOfferInResponse = resultingOfferResponse.Offers.FirstOrDefault(x => x.OfferId == offerDetailKey.OfferId);            

            //Assert
            Assert.NotNull(updatedOfferInResponse);

            foreach(var service in updatedOfferInResponse.OfferItems.First().Services)
            {
                Assert.Equal(2, service.PenaltyRef.ChangePenaltyRefIds.Count);
                Assert.Equal(2, service.PenaltyRef.CancellationPenaltyRefIds.Count);
            }

        }

        [Fact]
        public void UpdateOfferPrice_ShouldUpdateKeyOfferProductSegment_EightAmenities()
        {
            //Arrange
            int penaltyIndex = 1;
            int checkedBagCounter = 1;
            int serviceCounter = 1;
            IEnumerable<BaggageAllowances> baggageList = null;

            var brandMSResponseValue = TestDataProvider.GetTestData<BrandRS>("NDCOffer\\OfferPrice\\OfferPriceBrandMS\\BrandMSResponseValue");
            var offerDetailKey = TestDataProvider.GetTestData<OfferDetails>("NDCOffer\\OfferPrice\\OfferPriceBrandMS\\OfferDetailKey");
            var offerResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferPriceBrandMS\\OfferResponse");

            //Act
            var resultingOfferResponse = BrandMsRSMapper.UpdateOfferPrice(brandMSResponseValue, offerDetailKey, offerResponse, ref penaltyIndex, ref checkedBagCounter, ref serviceCounter, ref baggageList);

            var updatedOfferInResponse = resultingOfferResponse.Offers.FirstOrDefault(x => x.OfferId == offerDetailKey.OfferId);

            //Assert
            Assert.NotNull(updatedOfferInResponse);

            foreach (var service in updatedOfferInResponse.OfferItems.First().Services)
            {
                var serviceProductSegmentRefIds = service.ProductSegmentAssociation.OfferSegmentRefIds;
                foreach(var offerSegmentRefId in serviceProductSegmentRefIds)
                {
                    Assert.Equal(8, resultingOfferResponse.DataList.OfferSegments.First(x=> x.OfferSegmentId == offerSegmentRefId).ProductComponentRefIds.Count());
                }
            }
        }        
    }
}
