using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class ItaServiceCommandHandlerTests
    {
        private readonly Mock<IItaService> _iItaService;
        private readonly ItaRepoQueryHandler _handler;
        public ItaServiceCommandHandlerTests()
        {
            _iItaService = new Mock<IItaService>();
            _handler = new ItaRepoQueryHandler(_iItaService.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenAir4ObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ItaRepoQueryHandler(null));
        }
        [Fact]
        public async Task HandlerTest_Success()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<ItaRepoQuery>("OfferMS\\OrderReshopBundledValidRequest");

            TextReader xmlInput = new StreamReader("TestDataSamples/OfferMS/ItaServiceBundleSolutionResponse.xml");
            XmlSerializer deserialiser = new XmlSerializer(typeof(result));
            var searchResult = (result)deserialiser.Deserialize(xmlInput);

            _iItaService.Setup(i => i.GetOffers(It.IsAny<Infrastructure.Proxy.search>(), It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(searchResult);
            using var cancellationTokenSource = new System.Threading.CancellationTokenSource();

            //Act
            var result = await _handler.Handle(request, cancellationTokenSource.Token);

            //Assert

            Assert.NotNull(result.Items);
            Assert.Equal("ONL0c1V8Z8k4wVy87pwRgw1Tm", result.session);
            _iItaService.Verify(i => i.GetOffers(request.Search, cancellationTokenSource.Token), Times.Once);
        }

    }
}
