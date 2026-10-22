using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Operations.Models.Response;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Operations.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class FlightsQuery
    {
        [UseFiltering]
        [UseSorting]
        [GraphQLName("operationSearchFlights")]
        public async Task<IEnumerable<FlightInfo>> GetFlights(string airline, string direction, DateTime dateFrom, DateTime dateTo, string origin, [Service] IOAuthTokenProvider tokenProvider, [Service] IHttpConnector connector, [Service] ILogger<FlightInfo> logger)
        {
            if (tokenProvider is null)
                throw new ArgumentNullException(nameof(tokenProvider));

            if (connector is null)
                throw new ArgumentNullException(nameof(connector));


            string searchUrl = ConfigReader.Get(Common.Constants.Environment.FlightSearchUrl)
                                    .Replace("{airline}", airline)
                                    .Replace("{datefrom}", dateFrom.ToString(Common.Constants.Application.ISODateFormat))
                                    .Replace("{dateto}", dateTo.ToString(Common.Constants.Application.ISODateFormat))
                                    .Replace("{direction}", direction)
                                    .Replace("{origin}", origin);
            try
            {
                string token = await tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.FlightSearchAppId));
                var headers = new Dictionary<string, string>()
                    {
                        { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.FlightSearchOcpKey)}
                    };

                var response = await connector.SendAsync<FlightCollection>(searchUrl, headers, token, HttpMethod.Get, "");

                return response.Response.Flights;
            }
            catch (HttpRequestException ex)
            {
                var content = await ex.ReadResponseContentAsStringAsync();

                logger.LogError(ex, $"Search execution failed \r\n{content}");

                throw;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Unhandled Exception in Search \r\n{ex.Message}");

                throw;
            }
        }
    }
}
