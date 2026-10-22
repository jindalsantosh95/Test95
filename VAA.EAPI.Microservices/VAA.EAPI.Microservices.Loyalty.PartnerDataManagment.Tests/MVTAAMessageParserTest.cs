using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class MVTAAMessageParserTest
    {
        [Fact]
        public void TryParse_ValidAAMessage_ReturnsTrue()
        {
            var day = DateTime.UtcNow.Day;
            var payload =
            $@"MVT
            VS301/{day:D2}.GVBEL.LHR
            AA1512 EB1522
            SI LFO08";

            var result = MVTAAMessageParser.TryParse(payload, out var request);

            Assert.True(result);
            Assert.NotNull(request);
            Assert.Equal("VS", request.FlightCarrier);
            Assert.Equal(301, request.FlightNumber);
            Assert.Equal("LHR", request.ArrivalAirport);
        }

        [Fact]
        public void TryParse_InvalidAAMessage_ReturnsFalse()
        {
            var payload =
            @"MVT
            VS301/GVBEL.LHR
            AA1512 EB1522
            SI LFO08";

            var result = MVTAAMessageParser.TryParse(payload, out var request);

            Assert.False(result);
        }
    }
}