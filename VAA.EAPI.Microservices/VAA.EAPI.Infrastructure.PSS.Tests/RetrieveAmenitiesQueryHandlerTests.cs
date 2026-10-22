using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Products.Brand;
using VAA.EAPI.Infrastructure.PSS.Entities.RetrieveAmenities;
using VAA.EAPI.Infrastructure.PSS.Repository.Query;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using Xunit;

namespace VAA.EAPI.Infrastructure.PSS.Tests
{
    public class RetrieveAmenitiesQueryHandlerTests
    {
        private readonly Mock<IRetrieveAmenities> _retrievAmenities;
        private readonly RetrieveAmenitiesQueryHandler _handler;

        public RetrieveAmenitiesQueryHandlerTests()
        {
            _retrievAmenities = new Mock<IRetrieveAmenities>();
            _handler = new RetrieveAmenitiesQueryHandler(_retrievAmenities.Object);
        }

        [Fact]
        public void EmptyInterface_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RetrieveAmenitiesQueryHandler(null));
        }

        [Fact]
        public async Task Handler_ShouldReturnNullIfServiceReturnsNull()
        {
            //Arrange
            _retrievAmenities.Setup(x => x.Retrieve(It.IsAny<RetrieveAmenitiesRequest>())).ReturnsAsync(new RetrieveAmenitiesResponse() { FlightSegment = null });

            //Act
            var result = await _handler.Handle(new RetrieveAmenitiesQuery(), CancellationToken.None);

            //Assert
            Assert.Null(result);

        }

        [Fact]
        public async Task Handler_ShouldReturnValuesIfServiceReturnsValues()
        {
            //Arrange
            _retrievAmenities.Setup(x => x.Retrieve(It.IsAny<RetrieveAmenitiesRequest>())).ReturnsAsync(new RetrieveAmenitiesResponse() { FlightSegment = new List<FlightSegment>() { new FlightSegment() { Id = 1 } } });

            //Act
            var result = await _handler.Handle(new RetrieveAmenitiesQuery(), CancellationToken.None);

            //Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().Id);

        }
    }
}
