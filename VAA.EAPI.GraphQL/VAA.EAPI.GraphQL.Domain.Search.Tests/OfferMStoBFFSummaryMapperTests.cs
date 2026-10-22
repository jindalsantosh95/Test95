using System;
using System.Collections.Generic;
using VAA.EAPI.GraphQL.Domain.Search.Mapper;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    public class OfferMStoBFFSummaryMapperTests
    {
        [Fact]
        public void FareCalculationSummary_ExcludesInfant_AndCalculatesTotalAwardPoints_WithQuantity()
        {
            IReadOnlyList<Fare> fares =
            [
                new Fare { PassengerType = "ADULT", Quantity = 2, Price = new Price { AwardPoints = "95500" } },
                new Fare { PassengerType = "INFANT", Quantity = 1, Price = new Price { AwardPoints = "123" } },
                new Fare { PassengerType = "CHILD", Quantity = 1, Price = new Price { AwardPoints = "1000" } },
            ];

            var filteredFares = OfferMStoBFFSummaryMapper.ExcludeInfantFares(fares);

            Assert.DoesNotContain(filteredFares, f => string.Equals(f.PassengerType, "INFANT", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(2, filteredFares.Count);

            var totalAwardPoints = OfferMStoBFFSummaryMapper.CalculateTotalAwardPoints(filteredFares);
            Assert.Equal("192000", totalAwardPoints);
        }
    }
}

