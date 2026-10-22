using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Virgin.Models.Products.Brand;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.PSS.Entities.RetrieveAmenities;
using VAA.EAPI.Infrastructure.PSS.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Infrastructure.PSS.Tests
{
    public class RetrieveAmenitiesTests
    {
        private readonly Mock<IHttpConnector> _apiConnector;
        private readonly Mock<ILogger<RetrieveAmenities>> _logger;
        private readonly RetrieveAmenities _retrieveAmenities;
        
        public RetrieveAmenitiesTests()
        {
            _logger = new Mock<ILogger<RetrieveAmenities>>();
            _apiConnector = new Mock<IHttpConnector>();
            _retrieveAmenities = new RetrieveAmenities(_logger.Object, _apiConnector.Object);
            PSSTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void EmptyConnector_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() => new RetrieveAmenities(_logger.Object, null));
        }

        [Fact]
        public void EmptyLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() => new RetrieveAmenities(null, _apiConnector.Object));
        }

        [Fact]
        public async Task RetrieveAmenities_ShouldReturnResponse_WithGoodTokenAndAPIResponse()
        {
            //Arrange
            var flightSegments = new List<FlightSegment>() { new FlightSegment() { Id = 1 } };
            var tokenResponse = TestDataProvider.GetXmlTestData<BrandTokenResponse>("OrderReshopMS\\BrandAPITokenResponse");
            _apiConnector
                .Setup(x => x.SendAsync<RetrieveAmenitiesResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new RetrieveAmenitiesResponse() { FlightSegment = flightSegments });
            _apiConnector.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).ReturnsAsync(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent(Xml.Serialize(tokenResponse)) });

            //Act
            var result = await _retrieveAmenities.Retrieve(new RetrieveAmenitiesRequest());

            //Assert
            Assert.Equal(1, result.FlightSegment.First().Id);
        }
    }
}
