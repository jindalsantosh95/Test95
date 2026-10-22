using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]

    public class GetBookingQuery
    {
        private readonly ILogger<GetBookingQuery> _logger;
        protected readonly IOAuthTokenProvider _tokenProvider;
        protected readonly IHttpConnector _connector;

        public GetBookingQuery(ILogger<GetBookingQuery> logger, IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize(Roles = new[] { "Genesys.GetBooking" })]
        [GraphQLName("servicingGetBookingVAA")]
        [GraphQLDescription("Returns latest booking information for the selected fcNumber/phoneNo")]
        public async Task<GetBookingResponse> Getbooking(string fcNumber, string phoneNo)
        {
            try
            {
                _logger.LogDebug("GetBooking Call - Start");
                string uri = ConfigReader.Get(Common.Constants.Environment.GetBookingUrl);
                var getBookingResponse = await _connector.SendAsync<GetBookingResponse>(uri, GetHeaders(fcNumber, phoneNo), await GetToken(), HttpMethod.Get, string.Empty);
                _logger.LogDebug("GetBooking Call Successful - End ");

                return getBookingResponse.Response;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in GetBookingQuery - {ex.Message}", ex);
                throw;
            }
        }

        protected async Task<string> GetToken()
        {
            return await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.OrderSearchAppId));
        }

        protected static Dictionary<string, string> GetHeaders(string fcNumber, string phoneNo)
        {
            return new Dictionary<string, string>()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.OrderSearchOcpKey) },
                { "fCNumber",fcNumber },
                { "phoneNo",phoneNo }
            };
        }
    }
}
