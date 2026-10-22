using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice_Validations
{
    public class ValidatePaxListTests
    {
        [Fact]
        public void ValidatePaxList_ErrorFor_null_Request()
        {
            PaxType[] paxList = null;

            var validationErrors = ValidateOfferPrice.ValidatePaxList(paxList);

            Assert.True(validationErrors.Any());
            Assert.Equal("143_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidatePaxList_ErrorFor_NoPax_Request()
        {
            PaxType[] paxList = System.Array.Empty<PaxType>();

            var validationErrors = ValidateOfferPrice.ValidatePaxList(paxList);

            Assert.True(validationErrors.Any());
            Assert.Equal("143_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidatePaxList_ErrorForNoAdult_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            PaxType[] paxList = request.Request.DataLists.PaxList;

            foreach (var pax in paxList)
            {
                pax.PTC = "INF";
            }

            var validationErrors = ValidateOfferPrice.ValidatePaxList(paxList);

            Assert.True(validationErrors.Any());
            Assert.Equal("324_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidatePaxList_ErrorForCount_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            PaxType px1 = new()
            {
                PaxID = "ADULT_1",
                PTC = "INF"
            };
            PaxType px2 = request.Request.DataLists.PaxList.First();
            PaxType[] paxList = { px1, px2 };

            var validationErrors = ValidateOfferPrice.ValidatePaxList(paxList);

            Assert.True(validationErrors.Any());
            Assert.Equal("143_2", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidatePaxList_ErrorForMorethan9_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            PaxType[] paxList1 = request.Request.DataLists.PaxList;

            PaxType px7 = new()
            {
                PaxID = "AX7",
                PTC = "ADT"
            };
            PaxType px8 = new()
            {
                PaxID = "AX8",
                PTC = "GBE"
            };
            PaxType px9 = new()
            {
                PaxID = "CH9",
                PTC = "CHD"
            };
            PaxType px10 = new()
            {
                PaxID = "IN10",
                PTC = "GBE"
            };

            PaxType[] paxList2 = { px7, px8, px9, px10 };
            PaxType[] paxList = paxList1.Concat(paxList2).ToArray();

            var validationErrors = ValidateOfferPrice.ValidatePaxList(paxList);

            Assert.True(validationErrors.Any());
            Assert.Equal("68_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidatePaxList_ErrorForCountNotMatch_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            PaxType px1 = new()
            {
                PaxID = "IN1",
                PTC = "INF"
            };
            PaxType px2 = request.Request?.DataLists.PaxList.FirstOrDefault();
            PaxType px3 = new();
            PaxType[] paxList = { px1, px2, px3 };

            var validationErrors = ValidateOfferPrice.ValidatePaxList(paxList);

            Assert.True(validationErrors.Any());
            Assert.Equal("143_2", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidatePaxList_NoError_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            PaxType[] paxList = request.Request.DataLists.PaxList;

            var validationErrors = ValidateOfferPrice.ValidatePaxList(paxList);

            Assert.True(validationErrors.Count == 0);
        }
    }
}
