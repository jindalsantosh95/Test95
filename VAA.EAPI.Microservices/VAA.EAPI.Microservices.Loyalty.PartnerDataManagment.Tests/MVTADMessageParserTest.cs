using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class MVTADMessageParserTest
    {
        [Fact]
        public void TryParse_ValidADMessage_ReturnsTrue()
        {
            var day = DateTime.UtcNow.Day;

            var payload =
            $@"MVT
            VS301/{day:D2}.DEL
            AD1512 EB1522";

            var result = MVTADMessageParser.TryParse(payload, out var request);

            Assert.True(result);
            Assert.NotNull(request);
            Assert.Equal("VS", request.FlightCarrier);
            Assert.Equal(301, request.FlightNumber);
            Assert.Equal("DEL", request.DepartureAirport);
        }

        [Fact]
        public void TryParse_InvalidAAMessage_ReturnsFalse()
        {
            var day = DateTime.UtcNow.Day;

            var payload =
            $@"MVT
            VS301/{day:D2}.LHR
            AA1512 EB1522";

            var result = MVTADMessageParser.TryParse(payload, out var request);

            Assert.False(result);
        }
    }
}
