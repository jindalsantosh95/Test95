using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat;
using VAA.EAPI.GraphQL.Domain.Book.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests.Queries
{

    public class RetrieveSeatMapQueryTests
    {
        private Mock<ILogger<RetrieveSeatMapQuery>> _loggerMock;
        private Mock<IBasketServiceProvider> _basketServiceProviderMock;
        private Mock<IOfferServiceProvider> _offerServiceProviderMock;
        private Mock<IRefServiceProvider> _referenceServiceProviderMock;
        private Mock<ISeatServiceProvider> _seatServiceProviderMock;
        private Mock<ILOPAServiceProvider> _lopaServiceProviderMock;
        private Mock<ITokenB2CProvider<string>> _tokenB2CProviderMock;
        private Mock<ICustomerProvider> _customerProviderMock;
        private RetrieveSeatMapQuery _query;


        public RetrieveSeatMapQueryTests()
        {
            _loggerMock = new Mock<ILogger<RetrieveSeatMapQuery>>();
            _basketServiceProviderMock = new Mock<IBasketServiceProvider>();
            _offerServiceProviderMock = new Mock<IOfferServiceProvider>();
            _referenceServiceProviderMock = new Mock<IRefServiceProvider>();
            _seatServiceProviderMock = new Mock<ISeatServiceProvider>();
            _lopaServiceProviderMock = new Mock<ILOPAServiceProvider>();
            _tokenB2CProviderMock = new Mock<ITokenB2CProvider<string>>();
            _customerProviderMock = new Mock<ICustomerProvider>();

            _query = new RetrieveSeatMapQuery(
                _loggerMock.Object,
                _basketServiceProviderMock.Object,
                _offerServiceProviderMock.Object,
                _referenceServiceProviderMock.Object,
                _seatServiceProviderMock.Object,
                _lopaServiceProviderMock.Object,
                _tokenB2CProviderMock.Object,
                _customerProviderMock.Object
            );

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }


        [Fact]
        public void Constructor_NullDependencies_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RetrieveSeatMapQuery(
                null, _basketServiceProviderMock.Object, _offerServiceProviderMock.Object,
                _referenceServiceProviderMock.Object, _seatServiceProviderMock.Object,
                _lopaServiceProviderMock.Object, _tokenB2CProviderMock.Object,
                _customerProviderMock.Object));
        }

        [Fact]
        public async Task RetrieveSeatMap_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _query.RetrieveSeatMap(null));
        }

        [Fact]
        public async Task RetrieveSeatMap_EmptyBasketId_ThrowsArgumentException()
        {
            // Arrange
            var request = new SeatMapRequest { BasketId = "", SectorId = "SECTOR1" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _query.RetrieveSeatMap(request));
        }

        [Fact]
        public async Task RetrieveSeatMap_EmptySectorId_ThrowsArgumentExceptionAsync()
        {
            // Arrange
            var request = new SeatMapRequest { BasketId = "BASKET1", SectorId = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _query.RetrieveSeatMap(request));
        }

    }
}






