using VAA.EAPI.Microservices.Retail.Insurance.Domain;
using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteResponse;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Insurance.Tests
{
    public class InsuranceQuoteResponseMapperTests
    {

        [Fact]
        public void MapInsuranceQuoteResponse_ShouldReturnInsuranceQuoteResponse_WhenQuoteResponseIsNotNull()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<InsuranceQuoteRequest>("Insurance\\QuoteRequest");
            var response = TestDataProvider.GetTestData<QuoteResponse>("Insurance\\QuoteResponse");

            // Act
            var result = AllianzQuoteResponseMapper.MapInsuranceQuoteResponse(response, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Success", result.Status.Code);
            Assert.Equal("5336", result.Status.ProcessId);
            Assert.Equal("10/3/2024 4:58:28 PM", result.Status.CreationDate);
            Assert.Equal("3.0.9022.32238", result.Status.Version);
            var products = result.Products.ToArray();
            Assert.Single(products);
            Assert.Equal("001005319", products[0].ExternalCode);
            Assert.Equal("001005319", products[0].ProductCode);
            Assert.Equal("4293755080000336250", products[0].OfferId);
        }

    }

}