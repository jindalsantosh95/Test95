using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.PlusGrade;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.PlusGrade.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.PlusGrade
{
    public class BidForUpgradeOffersService : IBidForUpgradeOffersService
    {
        private readonly IHttpConnector _httpConnector;

        public BidForUpgradeOffersService(IHttpConnector httpConnector) => _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task<BidForUpgradeResponse> GetBidForUpgradeOffersAsync(BidForUpgradeRequest bidForUpgradeRequest)
        {
            var plusUpgradeURI = $"{ConfigReader.Get(Constant.Environment.PlusGradeApiUri).TrimEnd('/')}/{ConfigReader.Try<string>(Constant.Environment.PlusGradePartnerId)}/create";
            var plusGradeApiKey = ConfigReader.Get(Constant.Environment.PlusGradeApiKey);
            var headers = new Dictionary<string, string>
                {
                    {Constant.Application.PlusGrade_header_x_pnr,  bidForUpgradeRequest.Pnr},
                    {Constant.Application.PlusGrade_header_x_api_key, plusGradeApiKey},
                };
            bidForUpgradeRequest.ApiKey = plusGradeApiKey;

            return await _httpConnector.SendAsync<BidForUpgradeResponse>(plusUpgradeURI, headers: headers, bearerToken: null, HttpMethod.Post, content: Json.Serialize(bidForUpgradeRequest));
        }
    }
}
