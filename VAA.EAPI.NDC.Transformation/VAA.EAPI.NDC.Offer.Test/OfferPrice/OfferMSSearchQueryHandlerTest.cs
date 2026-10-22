using Moq;
using System.Net;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMSSearchQuery;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice
{
    public class OfferSearchMSQueryHandlerTests
    {
        private readonly OfferSearchMSQueryHandler _handler;
        private readonly Mock<IHttpConnector> _mockHttpConnector;

        public OfferSearchMSQueryHandlerTests()
        {
            _mockHttpConnector = new Mock<IHttpConnector>();
            _handler = new OfferSearchMSQueryHandler(_mockHttpConnector.Object);
            ConfigReader.Set("offermssubscriptionkey", "Test_offermssubscriptionkey");
            ConfigReader.Set("offersearchendpoint", "https://testresources.virginatlantic.com/SIT/OfferMicroservice/Offers/");
            
        }

        [Fact]
        public async Task Handle_ShouldReturnOfferResponse_WhenHttpResponseIsSuccessful()
        {

            var query = TestDataProvider.GetTestData<OfferMSSearchQuery>("OfferMS\\OfferSearchQueryRQ");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse");
            var responseContent = Json.Serialize(offerDoc);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            _mockHttpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), HttpMethod.Get, It.IsAny<string>(),It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("AMS", result.DataList.FlightSegments.First().DestinationAirportCode);
            Assert.Equal("7048", result.DataList.FlightSegments.First().MarketingFlightNumber);
            Assert.Equal("LHR", result.DataList.FlightSegments.First().OriginAirportCode);
        }

        [Fact]
        public async Task Handle_ShouldReturnErrorResponse_WhenHttpResponseIsNotSuccessful()
        {
            var query = TestDataProvider.GetTestData<OfferMSSearchQuery>("OfferMS\\OfferSearchQueryRQ");
            var errorContent = Json.Serialize(new List<ErrorDetails> { new ErrorDetails { Code = "400", Message = "Bad Request" } });

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(errorContent)
            };

            _mockHttpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), HttpMethod.Get, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("400", result.Error.First().Code);
        }

    }
}
