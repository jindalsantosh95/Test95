using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice_Validations
{
    public class ValidateOfferIdTests
    {
        [Fact]
        public void ValidateOfferId_ErrorFor_null_Request()
        {
            List<SelectedOfferItemType> offerIdSummary = null;

            var validationErrors = ValidateOfferPrice.ValidateOfferId(offerIdSummary);

            Assert.True(validationErrors.Any());
            Assert.Equal("OFFERPRICE_OfferRefID_InValid", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidateOfferId_NoError_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
           
            var pricedOffer = (SelectedOfferListType)request.Request.PricedOffer.Item;
            var validationErrors = ValidateOfferPrice.ValidateOfferId(pricedOffer?.SelectedOffer?.FirstOrDefault()?.SelectedOfferItem?.ToList());

            Assert.True(validationErrors.Count == 0);

        }

        [Fact]
        public void ValidateOfferId_ErrorFor_NullOrWhiteSpace_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            
            var pricedOffer = (SelectedOfferListType)request.Request.PricedOffer.Item;
            var offerIdSummary = pricedOffer.SelectedOffer.FirstOrDefault()?.SelectedOfferItem?.ToList();

            offerIdSummary.First().OfferItemRefID = "";

            var validationErrors = ValidateOfferPrice.ValidateOfferId(offerIdSummary);
            
            Assert.True(validationErrors.Any());
            Assert.Equal("OFFERPRICE_OfferRefID_InValid", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidateOfferItemRefId_NoError_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            var selectedOfferList = (SelectedOfferListType)request.Request.PricedOffer.Item;
            var selectedOffer = selectedOfferList.SelectedOffer.First();

            var offerDb = new OfferResponse
            {
                Offers = new List<OfferDetails>
                {
                    new OfferDetails
                    {
                        OfferId = selectedOffer.OfferRefID,
                        OfferItems = selectedOffer.SelectedOfferItem.Select(x => new OfferItem
                            {
                                OfferItemId = x.OfferItemRefID,
                                ServiceBeneficiaries = x.PaxRefID?.ToList()
                            }).ToList()
                    }
                }
            };

            var validationErrors = ValidateOfferPrice.ValidateOfferItemRefId(offerDb, request);

            Assert.Empty(validationErrors);
        }

        [Fact]
        public void ValidateOfferItemRefId_ErrorFor_InvalidOfferItemRefId_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            var selectedOfferList = (SelectedOfferListType)request.Request.PricedOffer.Item;
            var offerRefId = selectedOfferList.SelectedOffer.First().OfferRefID;
            var validOfferItemId = selectedOfferList.SelectedOffer.First().SelectedOfferItem.First().OfferItemRefID;

            selectedOfferList.SelectedOffer.First().SelectedOfferItem.First().OfferItemRefID = "INVALID_OFFER_ITEM_REF_ID";

            var offerDb = new OfferResponse
            {
                Offers = new List<OfferDetails>
                {
                    new OfferDetails
                    {
                        OfferId = offerRefId,
                        OfferItems = new List<OfferItem>    
                        {
                            new OfferItem { OfferItemId = validOfferItemId }
                        }
                     }   
                }
            };

            var validationErrors = ValidateOfferPrice.ValidateOfferItemRefId(offerDb, request);

            Assert.True(validationErrors.Any());
            Assert.Equal("913_3", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidateOfferItemRefId_ErrorFor_NullOrEmptyOrWhiteSpace_OfferItemRefId_Request()
        {
            var invalidValues = new[] { (string)null, string.Empty, " " };

            foreach (var invalidValue in invalidValues)
            {
                var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
                var selectedOfferList = (SelectedOfferListType)request.Request.PricedOffer.Item;

                var offerRefId = selectedOfferList.SelectedOffer.First().OfferRefID;
                var validOfferItemIds = selectedOfferList.SelectedOffer.First().SelectedOfferItem.Select(x => x.OfferItemRefID).ToList();

                selectedOfferList.SelectedOffer.First().SelectedOfferItem.First().OfferItemRefID = invalidValue;

                var offerDb = new OfferResponse
                {
                    Offers = new List<OfferDetails>
                    {
                        new OfferDetails
                        {
                            OfferId = offerRefId,
                            OfferItems = validOfferItemIds.Select(x => new OfferItem { OfferItemId = x }).ToList()
                        }
                    }
                };

                var validationErrors = ValidateOfferPrice.ValidateOfferItemRefId(offerDb, request);

                Assert.True(validationErrors.Any());
                Assert.Equal("913_3", validationErrors.First().ErrorCode);
            }
        }

        [Fact]
        public void ValidateOfferItemRefId_ErrorFor_Null_SelectedOfferItem_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            var selectedOfferList = (SelectedOfferListType)request.Request.PricedOffer.Item;

            var offerRefId = selectedOfferList.SelectedOffer.First().OfferRefID;
            var validOfferItemIds = selectedOfferList.SelectedOffer.First().SelectedOfferItem.Select(x => x.OfferItemRefID).ToList();

            selectedOfferList.SelectedOffer.First().SelectedOfferItem = null;

            var offerDb = new OfferResponse
            {
                Offers = new List<OfferDetails>
                {
                    new OfferDetails
                    {
                        OfferId = offerRefId,
                        OfferItems = validOfferItemIds.Select(x => new OfferItem { OfferItemId = x }).ToList()
                    }
                }
            };

            var validationErrors = ValidateOfferPrice.ValidateOfferItemRefId(offerDb, request);

            Assert.True(validationErrors.Any());
            Assert.Equal("913_3", validationErrors.First().ErrorCode);
        }
    }
}
