using MediatR;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Common.OfferSearch;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class OfferSearchQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly OfferSearchQueryHandler _handler;

        public OfferSearchQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _handler = new OfferSearchQueryHandler(_mediator.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OfferSearchQueryHandler(null));
        }

        [Fact]
        public async Task Handler_ShouldReturnResponse_WithReturnSuccess()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OfferSearchQuery>("OfferMS//OfferSearchQueryRQ");
            var reshopResponse = TestDataProvider.GetTestData<OfferResponse>("OfferMS//ReshopResponseOfferDB");
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_ShouldReturnResponse_WithReturnError()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OfferSearchQuery>("OfferMS//OfferSearchQueryRQ");
            var reshopResponse = new OfferResponse() { Offers = null };
            _mediator.Setup(x => x.Send(It.IsAny<OfferSearchRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reshopResponse);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Constants.ErrorCodes.InvalidOfferRefId, result.Value.Errors.FirstOrDefault().Code);
        }

    }
}
