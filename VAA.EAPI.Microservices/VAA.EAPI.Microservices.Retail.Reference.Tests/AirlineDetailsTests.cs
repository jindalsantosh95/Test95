using Newtonsoft.Json;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Reference.Tests
{
    public sealed class AirlineDetailsTests
    {
        [Fact]
        public void AirlineDetails_DeserializesUrlAlongsideCarrierDetails()
        {
            const string json = "{\"airlineDetails\":[{\"carrierCode\":\"AA\",\"carrierName\":\"American Airlines\",\"url\":\"https://www.aa.com/homePage.do\"}]}";

            var result = JsonConvert.DeserializeObject<AirlineReferenceData>(json);
            var airline = Assert.Single(result!.AirlineDetails);

            Assert.Equal("AA", airline.CarrierCode);
            Assert.Equal("American Airlines", airline.CarrierName);
            Assert.Equal("https://www.aa.com/homePage.do", airline.Url);
        }
    }
}
