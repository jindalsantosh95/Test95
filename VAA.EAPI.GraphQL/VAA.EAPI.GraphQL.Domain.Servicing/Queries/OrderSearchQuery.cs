using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]

    public class OrderSearchQuery
    {
        private readonly ILogger<OrderSearchQuery> _logger;
        protected readonly IOAuthTokenProvider _tokenProvider;
        protected readonly IHttpConnector _connector;

        public OrderSearchQuery(IOAuthTokenProvider tokenProvider, IHttpConnector connector, ILogger<OrderSearchQuery> logger)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("servicingGetOrder")]
        [GraphQLDescription("Returns order information for the selected PNR (PNR set in authorization token).")]

        public async Task<OrderSearchResponse> GetOrderSearch()
        {
            Claim claimDetails = null;

            try
            {
                var claimType = AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>()?.Principal.Claims.First(x => x.Type == Common.Constants.Application.ClaimType).Value;

                if (claimType.IsNullOrWhiteSpace()) throw new SecurityTokenException();

                claimDetails = Json.Deserialize<Claim>(claimType);

                //Calling OrderSearch for data

                var orderSearchResponse = await OrderSearch(claimDetails);

                //Calling Identity for new token

                var token = await Identity(claimDetails, orderSearchResponse.Response);

                AsyncContext.Current.Context.Items.Add(new KeyValuePair<object, object>(Common.Constants.Application.ResponseHeaderKey, new Dictionary<string, string?>() { { "Authorization", token.Headers.GetValues("Authorization").FirstOrDefault() } }));

                return orderSearchResponse.Response;
            }
            catch (Exception ex)
            {
                if (claimDetails is not null)
                {
                    ex.Data.Add("PNR", claimDetails.RLOC);
                    ex.Data.Add("Ticket", claimDetails.TicketRef);
                }

                _logger.LogError(ex, $"Error occurred in OrderSearchQuery - {ex.Message}", ex);

                throw;
            }
        }

        protected async Task<string> GetToken() => await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.OrderSearchAppId));

        protected Dictionary<string, string?> GetOrderSearchHeaders(Claim claimDetails)
        {
            return new Dictionary<string, string?>()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.OrderSearchOcpKey)},
                {"ChannelId",Common.Constants.Application.SelfServiceChannelId },
                {"HashKey", claimDetails.HashKey},
            };
        }

        protected Dictionary<string, string?> GetIdentityHeaders(Claim claim, OrderSearchResponse orderSearchResponse)
        {
            return new Dictionary<string, string?>()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.IdentityOcpKey) },
                {"FirstName",orderSearchResponse?.CustomerDetails?.First(x => Hash.SHA256Managed($"{x.FirstName?.ToUpper()}{x.LastName?.ToUpper()}") == claim.HashKey).FirstName },
                {"LastName" ,orderSearchResponse?.CustomerDetails?.First(x => Hash.SHA256Managed($"{x.FirstName?.ToUpper()}{x.LastName?.ToUpper()}") == claim.HashKey).LastName },
                {"Pnr" , claim.RLOC },
                {"Type" , claim.Type },
                {"TicketRef",claim.TicketRef }
            };
        }

        protected static OrderSearchRequest ClaimToRequestMapper(Claim claim)
        {
            return new OrderSearchRequest()
            {
                SearchFilterCriteria = new SearchFilterCriteria()
                {
                    bookingReferenceCriteria = new BookingReferenceCriteria()
                    {
                        BookingReference = new BookingReference()
                        {
                            RLOC = claim.RLOC,
                            RLOCCreateDate = claim.CreationDate
                        }
                    },
                    accountableDocumentFilterCriteria = new AccountableDocumentFilterCriteria()
                    {
                        AccountableDocumentReference = new AccountableDocumentReference()
                        {
                            DocumentNumber = claim.TicketRef,
                            IssueDate = claim.CreationDate
                        }
                    }
                }
            };
        }

        protected async Task<APIResponse<OrderSearchResponse>> OrderSearch(Claim claimDetails)
        {
            string orderSearchUri = ConfigReader.Get(Common.Constants.Environment.OrderSearchUrl);

            var orderSearchResponse = await _connector.SendAsync<OrderSearchResponse>(orderSearchUri, GetOrderSearchHeaders(claimDetails), await GetToken(), HttpMethod.Post, Json.Serialize(ClaimToRequestMapper(claimDetails)));

            return orderSearchResponse;
        }

        protected async Task<APIResponse<string>> Identity(Claim claimDetails, OrderSearchResponse orderSearchResponse)
        {
            var token = await _connector.SendAsync<string>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.TokenEndpoint}", GetIdentityHeaders(claimDetails, orderSearchResponse), await GetTokenForIdentityMS(), HttpMethod.Post, string.Empty);

            return token;
        }

        protected async Task<string> GetTokenForIdentityMS() => await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.IdentityMSAppId));
    }
}
