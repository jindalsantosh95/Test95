using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.Microservices.Book
{
    public class InsuranceQuote
    {
        [Fact]
        [Trait("Category", Traits.BookMicroserviceSmoke)]
        [Trait("Category", Traits.BookFeatureInsurance)]
        public async Task CanGetInsuranceQuote()
        {
            var requestData = File.ReadAllText(@"requests\Body\Microservices\InsuranceQuote.json");
            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(requestData);

            string departureDate = DateTime.Now.AddMonths(3).ToString("yyyy-MM-ddT00:00:00");
            string returnDate = DateTime.Now.AddMonths(3).AddDays(14).ToString("yyyy-MM-ddT00:00:00");

            payloadObject.SelectToken("travel.departureDate").Replace(departureDate);
            payloadObject.SelectToken("travel.returnDate").Replace(returnDate);

            var payload = JsonConvert.SerializeObject(payloadObject);

            HttpResponseMessage insuranceQuoteHttpResponse = MicroserviceEndpoint.InsuranceQuote(payload);
            var insuranceResponse = JsonConvert.DeserializeObject<JToken>(await insuranceQuoteHttpResponse.Content.ReadAsStringAsync());
            var insurancePrice = insuranceResponse.SelectToken("products[0].prices[0].price");

            Assert.True(insurancePrice != null, $"Insurance price was not returned.\n{insuranceResponse}");
            Assert.True(decimal.Parse(insurancePrice.ToString()) > 0, $"Insurance price was not more than £0.\nInsurance price: {insurancePrice}");
        }
    }
}