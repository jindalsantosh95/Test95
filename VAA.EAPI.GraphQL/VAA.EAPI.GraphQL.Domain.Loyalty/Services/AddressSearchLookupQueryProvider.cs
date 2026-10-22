using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.AddressSearch;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class AddressSearchLookupQueryProvider : IAddressSearchLookupQueryProvider
    {
        private readonly IOAuthTokenProvider _oauthTokenProvider;
        private readonly IHttpConnector _httpConnector;


        public AddressSearchLookupQueryProvider(IOAuthTokenProvider oauthTokenProvider, IHttpConnector httpConnector)
        {
            _oauthTokenProvider = oauthTokenProvider ?? throw new ArgumentNullException(nameof(oauthTokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<List<AddressSearchbyPostcodeResponse>> GetAddressSearchbyPostcode(AddressSearchbyPostcodeRequest request)
        {
            if ((request?.CountryCode == null || request?.CountryCode == string.Empty) || (request?.PostCode == null || request?.PostCode == string.Empty))
                throw new ArgumentNullException(nameof(request));
            
            string pincodeAddressSearchUri = $"{ConfigReader.Get(Common.Constants.Environment.AddressSearchbyPostcodeUrl)}{Common.Constants.Application.AddressSearchbyPostcodeEndPoint}".Replace("{addressText}", request.PostCode).Replace("{CountryCode}", request.CountryCode);
            var apiResponse = await _httpConnector.GetAsync<List<AddressSearchbyPostcodeResponse>>(pincodeAddressSearchUri, GetHeaders(), await _oauthTokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.AddressSearchAPIAppId)));
            return apiResponse.Response;
        }

        public async Task<List<AddressSearchbyRetrieveResponse>> GetAddressSearchbyRetrieve(string addressId)
        {
            if (addressId == null || addressId == string.Empty)
                throw new ArgumentNullException(nameof(addressId));

            string retrieveAddressSearchUri = $"{ConfigReader.Get(Common.Constants.Environment.AddressSearchbyPostcodeUrl)}{Common.Constants.Application.AddressSearchbyRetriveEndPoint}".Replace("{addressId}", addressId);
            var apiResponse = await _httpConnector.GetAsync<List<AddressSearchbyRetrieveResponse>>(retrieveAddressSearchUri, GetHeaders(), await _oauthTokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.AddressSearchAPIAppId)));
            return apiResponse.Response;
        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>()
            {
                {  Common.Constants.Application.OcpKeyHeader , ConfigReader.Get(Common.Constants.Environment.EnrolmentMSOcpKey) }
            };
        }
    }
}