using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Search.Models;
using VAA.EAPI.GraphQL.Infrastructure.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.FlightSearch
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class FlightsQuery
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IMicroserviceConnector _connector;

        public FlightsQuery(IOAuthTokenProvider tokenProvider, IMicroserviceConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));

        }

        [UseFiltering]
        [UseSorting]
        public async Task<IEnumerable<Models.Flight>> GetFlights(string airline, string direction, DateTime dateFrom, DateTime dateTo, string origin, [Service] IOAuthTokenProvider tokenProvider, [Service] IMicroserviceConnector connector, [Service] ILogger<Models.Flight> logger)
        {
            if (tokenProvider is null)
                throw new ArgumentNullException(nameof(tokenProvider));

            if (connector is null)
                throw new ArgumentNullException(nameof(connector));


            string searchUrl = ConfigReader.GetConfigurationSettings(Common.Constants.Environment.FlightSearchUrl)
                                    .Replace("{airline}", airline)
                                    .Replace("{datefrom}", dateFrom.ToString(Common.Constants.Application.FlightSearchDateFormat))
                                    .Replace("{dateto}", dateTo.ToString(Common.Constants.Application.FlightSearchDateFormat))
                                    .Replace("{direction}", direction)
                                    .Replace("{origin}", origin);
            try
            {
                string token = await _tokenProvider.AcquireTokenAsync(ConfigReader.GetConfigurationSettings(Common.Constants.Environment.FlightSearchAppId));
                var headers = new Dictionary<string, string>()
                    {
                        { Common.Constants.Application.OcpKeyHeader, ConfigReader.GetConfigurationSettings(Common.Constants.Environment.FlightSearchOcpKey)}
                    };

                var response = await connector.SendAsync<FlightCollection>(searchUrl, headers, token, HttpMethod.Get, "");

                return response.Response.Flights;
            }
            catch (EAPIConnectionException ex)
            {
                logger.LogError(ex, $"Search execution failed \r\n{ex.APIResponse}");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Unahandled Exception in Search \r\n{ex.Message}");
                throw;
            }

        }


    }
}
