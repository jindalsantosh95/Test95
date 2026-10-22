using Newtonsoft.Json.Linq;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips;

namespace VAA.EAPI.Microservices.Retail.Holidays.Tests
{
    public class HotelDetailsTests
    {
        [Fact]
        public void ExplicitOperator_SetsName_WithoutHighlightSuffix()
        {
            var hotelName = "Virgin Hotels Las Vegas - Curio Collection by Hilton";
            var highlight = "King Chamber Roll-in Shower";

            var source = new JObject
            {
                ["summary"] = new JObject
                {
                    ["name"] = hotelName,
                    ["highlight"] = highlight
                }
            };

            var details = (HotelDetails)source;

            Assert.NotNull(details);
            Assert.Equal(hotelName, details.Name);
        }
    }
}
