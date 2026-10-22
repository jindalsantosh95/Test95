using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Application = VAA.EAPI.GraphQL.Common.Constants.Application;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class PnrQueryProvider : IPnrQueryProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public PnrQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<IEnumerable<string>> RetrievePNRsbyEmail(string emailAddress)
        {
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.OrderDataAppId));

            var flightBookingDetailsbyEmail = (await _connector.SendAsync<OrderBookingResponse>(ConfigReader.Get(Environment.OrderFutureBookingUrl), GetHeaders(string.Empty, string.Empty, emailAddress, string.Empty, string.Empty, ConfigReader.Get(Environment.OrderDataOcpKey)), token, HttpMethod.Get, string.Empty)).Response.FlightBookingsDetails;

            return flightBookingDetailsbyEmail?.Select(x => x.BookingReference)?.Distinct();
        }

        private static Dictionary<string, string> GetHeaders(string memberNumber, string phoneNo, string email, string page, string pageSize, string OcpKey)
        {
            return new Dictionary<string, string>
                {
                    { Application.FcNumber, memberNumber },
                    { Application.PhoneNo, phoneNo },
                    { Application.Email, email },
                    { Application.OcpKeyHeader, OcpKey },
                    { Application.Page, page },
                    { Application.PageSize, pageSize }
                };
        }
    }
}
