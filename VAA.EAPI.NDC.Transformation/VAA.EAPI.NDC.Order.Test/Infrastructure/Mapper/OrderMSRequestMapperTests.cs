using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.Infrastructure.Mapper
{
    public class OrderMSRequestMapperTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void MapOrderMSRequest_UsesResidenceCountryForNationality_WhenCitizenshipCountryIsMissing(string? citizenshipCountryCode)
        {
            // Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            var offerDocument = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\OfferDoc_Return_2ADT1CHD1GBE");
            var identityDocument = request.Request.DataLists.PaxList.First().IdentityDoc.First();
            identityDocument.CitizenshipCountryCode = string.IsNullOrWhiteSpace(identityDocument.CitizenshipCountryCode) ? identityDocument.IssuingCountryCode : identityDocument.CitizenshipCountryCode;

            // Act
            var result = OrderMSRequestMapper.MapOrderMSRequest(request, offerDocument, false, null);

            // Assert
            var travelDocument = result.DataList.CustomerDetails.First().TravelDocuments.First();
            Assert.Equal(identityDocument.CitizenshipCountryCode, travelDocument.CountryOfIssue);
        }

        [Fact]
        public void MapOrderMSRequest_UsesCitizenshipCountryForNationality_WhenCitizenshipCountryIsPresent()
        {
            // Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            var offerDocument = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\OfferDoc_Return_2ADT1CHD1GBE");
            var identityDocument = request.Request.DataLists.PaxList.First().IdentityDoc.First();
            identityDocument.CitizenshipCountryCode = string.IsNullOrWhiteSpace(identityDocument.CitizenshipCountryCode) ? identityDocument.IssuingCountryCode : identityDocument.CitizenshipCountryCode;

            // Act
            var result = OrderMSRequestMapper.MapOrderMSRequest(request, offerDocument, false, null);

            // Assert
            var travelDocument = result.DataList.CustomerDetails.First().TravelDocuments.First();
            Assert.Equal("USA", travelDocument.CountryOfIssue);
        }
    }
}
