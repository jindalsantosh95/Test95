using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class MemberProfileMutationProvider<T> : IMemberProfileMutationProvider<T>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public MemberProfileMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider;
            _connector = connector;
        }

        public async Task<string> UpdateMemberProfile(MemberProfileUpdateRequest updateMemberProfileRequest)
        {
            string customerUri = $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl)}{Common.Constants.Application.VACMemberUpdateEndPoint}";

            Dictionary<string, string> headers = new()
                    {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) }
                            };

            return (await _connector.SendAsync<string>(customerUri, headers, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId)), HttpMethod.Patch, Json.Serialize(updateMemberProfileRequest))).Response;
        }
    }
}
