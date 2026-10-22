using Moq;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests
{
    public class OfferServiceProviderTests
    {

        private readonly OfferServiceProvider _offerServiceProvider;
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;
        private readonly Mock<IOfferServiceProvider> _iofferServiceProvider;

        public OfferServiceProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();
            _iofferServiceProvider = new Mock<IOfferServiceProvider>();

            _offerServiceProvider = new OfferServiceProvider(_mockConnector.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException_OfferServiceProvider()
        {
            Assert.Throws<ArgumentNullException>(() => new OfferServiceProvider(null));
        }

        [Fact(Skip = "Had missing provider initialisation and has missing config")]
        public async Task OfferServiceProviderShouldThrowException()
        {
            //Arrange
            var offerId = "";
            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity };
            _iofferServiceProvider.Setup(x => x.OfferRetrieveAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _offerServiceProvider.OfferRetrieveAsync(offerId));
        }
    }
}
