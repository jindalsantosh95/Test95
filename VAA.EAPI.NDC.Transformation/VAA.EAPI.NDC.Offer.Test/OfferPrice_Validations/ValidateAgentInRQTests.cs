using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice_Validations
{
    public class ValidateAgentInRQTests
    {
        [Fact]
        public void ValidateAgentInRQ_NoError_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            var validationErrors = ValidateOfferPrice.ValidateAgentInRQ(request.DistributionChain);

            Assert.True(validationErrors.Count == 0);

        }

        [Fact]
        public void ValidateAgentInRQ_Error_ForNullOrg_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            var badRequest = request.DistributionChain;

            badRequest.FirstOrDefault().ParticipatingOrg = null;
            var validationErrors = ValidateOfferPrice.ValidateAgentInRQ(badRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentInRQ_Error_ForNullOrgId_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            var badRequest = request.DistributionChain;

            badRequest.First().ParticipatingOrg.OrgID = "";
            var validationErrors = ValidateOfferPrice.ValidateAgentInRQ(badRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentInRQ_Error_ForSalesAgent_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            request.DistributionChain.First().SalesAgent = new() { SalesAgentID = string.Empty };

            var validationErrors = ValidateOfferPrice.ValidateAgentInRQ(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentInRQ_Error_ForSalesBranch_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            request.DistributionChain.First().SalesBranch = new() { SalesBranchID = string.Empty };

            var validationErrors = ValidateOfferPrice.ValidateAgentInRQ(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentInRQ_Error_ForZeroOrdinal_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            
            request.DistributionChain.First().Ordinal = "0";
           
            var validationErrors = ValidateOfferPrice.ValidateAgentInRQ(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_24", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentInRQ_Error_ForEmptyOrdinal_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            
            request.DistributionChain.First().Ordinal = string.Empty;
            
            var validationErrors = ValidateOfferPrice.ValidateAgentInRQ(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_24", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentInRQ_Error_ForCharOrdinal_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            request.DistributionChain.First().Ordinal = "abc";
            var validationErrors = ValidateOfferPrice.ValidateAgentInRQ(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_24", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentInRQ_Error_ForOrdinalNotStarting1_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            request.DistributionChain.First().Ordinal = "2";
            
            var validationErrors = ValidateOfferPrice.ValidateAgentInRQ(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_24", validationErrors.First().ErrorCode);

        }
    }
}
