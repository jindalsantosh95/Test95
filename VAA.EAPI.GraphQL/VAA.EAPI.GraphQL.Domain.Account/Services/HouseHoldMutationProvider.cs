using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class HouseHoldMutationProvider : IHouseHoldMutationProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        private readonly static Dictionary<string, string> headers = new Dictionary<string, string>()
        {
            { Common.Constants.Application.OcpKeyHeader,  ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey)}
        };

        public HouseHoldMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<ResponseMessage> HouseHoldLinking(HouseHoldLinkingRequest houseHoldLinkingRequest)
        {
            string houseHoldLinkingUri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.HouseholdLinkingEndPoint}";

            return (await _connector.SendAsync<ResponseMessage>(houseHoldLinkingUri, headers, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId)), HttpMethod.Post, Json.Serialize(houseHoldLinkingRequest))).Response;
        }
    }
}
