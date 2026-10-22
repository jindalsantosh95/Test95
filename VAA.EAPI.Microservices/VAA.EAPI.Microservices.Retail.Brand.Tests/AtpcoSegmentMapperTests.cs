using System;
using VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand;
using VAA.EAPI.Microservices.Retail.Brand.Infrastructure.ATPCO.Request;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Brand.Tests
{
    public class AtpcoSegmentMapperTests
    {
        [Fact]
        public void ExplicitOperator_AppendsTicketDesignatorToFareBasis()
        {
            var result = (Segment)CreateSegment("UWF1");

            Assert.Equal("NGCEB8WA/UWF1", result.Fbc);
        }

        [Fact]
        public void ExplicitOperator_UsesFareBasisWhenTicketDesignatorIsNotProvided()
        {
            var result = (Segment)CreateSegment(null);

            Assert.Equal("NGCEB8WA", result.Fbc);
        }

        private static BrandQueryRequestSegment CreateSegment(string? ticketDesignator) => new()
        {
            Carrier = "VS",
            FlightNumber = 1,
            DepartureDate = new DateTime(2026, 1, 1),
            Origin = "LHR",
            Destination = "JFK",
            Cabin = 1,
            FareBasis = "NGCEB8WA",
            TicketDesignator = ticketDesignator
        };
    }
}
