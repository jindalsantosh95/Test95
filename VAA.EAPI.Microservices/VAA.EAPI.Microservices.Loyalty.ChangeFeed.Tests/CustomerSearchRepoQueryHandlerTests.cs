using MediatR;
using Moq;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Repository;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Tests
{
    public class CustomerSearchRepoQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public CustomerSearchRepoQueryHandlerTests()
        {
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchRepoQueryHandler(null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerSearch\\ListOfCustomerSearchDocs");
            
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderRepoQuery>(), CancellationToken.None)).ReturnsAsync(mockResponse);
            
            CustomerSearchRepoQuery query = new CustomerSearchRepoQuery("9876543210","Customer");
            CustomerSearchRepoQueryHandler handler = new CustomerSearchRepoQueryHandler(_mockMediator.Object);

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Gosslin", ((JObject)result.First())["FirstName"]);
            Assert.Equal("Jamesaa", ((JObject)result.First())["LastName"]);
            Assert.Equal("test222555@gmail.com", ((JObject)result.First())["EmailAddress"]);
            Assert.Equal("1074714856", ((JObject)result.First())["LoyaltyMemberNumber"]);
            Assert.Equal("D47166A3-533D-9159-E053-6941D90A1989", ((JObject)result.First())["CustomerId"]);
        }
    }
}
