using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Mapper;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public sealed class FlownPassengerParameterMapperTests
    {
        [Fact]
        public void Create_ThrowsWithColumnName_WhenARequiredFieldIsMissing()
        {
            var request = new FlownPassengerRequest
            {
                EventFlight = new Flight
                {
                    FlightCarrier = "VS",
                    FlightNumber = "0209",
                    DepartureAirport = "ICN",
                    ArrivalAirport = "LHR",
                    DepartureDate = new DateTime(2026, 9, 27)
                },
                TicketNumber = "9325019040303"
            };

            var exception = Assert.Throws<MissingRequiredFlownPassengerFieldException>(() => FlownPassengerParameterMapper.Create(request));

            Assert.Equal("FD_LAST_NAME", exception.ColumnName);
        }
    }
}
