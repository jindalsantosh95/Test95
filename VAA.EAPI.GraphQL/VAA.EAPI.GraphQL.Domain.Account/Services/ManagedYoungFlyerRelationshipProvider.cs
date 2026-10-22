using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManagedYoungFlyer;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Application = VAA.EAPI.GraphQL.Common.Constants.Application;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public sealed class ManagedYoungFlyerRelationshipProvider : IManagedYoungFlyerRelationshipProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public ManagedYoungFlyerRelationshipProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<IReadOnlyCollection<string>> GetManagedYoungFlyersCustomerIdsAsync(string parentCustomerId)
        {
            var url = $"{ConfigReader.Get(Environment.LoyaltyMemberManagementMsUrl)}{Application.MemberHouseholdRetrievalEndPoint}".Replace("{custid}", parentCustomerId);
            var headers = new Dictionary<string, string> { { Application.OcpKeyHeader, ConfigReader.Get(Environment.MemberManagementOcpKey) } };
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.MemberManagementAppId));
            var response = (await _connector.GetAsync<HouseholdDetailsResponse>(url, headers, token)).Response;

            return response?.HouseholdMembers?
                .Where(x => x.RelationshipType.Is(VAA.EAPI.GraphQL.Domain.Account.Constants.Application.LinkedMemberChild) && !x.CustomerId.IsNullOrWhiteSpace())
                .Select(x => x.CustomerId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? [];
        }
    }
}
