using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Constants;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.FutureFlights;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class FutureFlightsQueryProvider : IFutureFlightsQueryProvider
    {
        private readonly IHttpConnector _connector;
        private readonly ILoyaltyReferenceDataProvider _referenceDataProvider;
        private readonly ILogger<FutureFlightsQueryProvider> _logger;

        public FutureFlightsQueryProvider(IHttpConnector connector, ILoyaltyReferenceDataProvider referenceDataProvider, ILogger<FutureFlightsQueryProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _referenceDataProvider = referenceDataProvider ?? throw new ArgumentNullException(nameof(referenceDataProvider));
        }

        public async Task<FlightResponse> Query(FlightRequest flightRequest)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.OrderDataMSUrl)}{Common.Constants.Application.BookingsEndpoint}";

            Dictionary<string, string> headers = new()
                            {
                                  { Application.FCNumber, flightRequest.MemberNumber},
                                  { Application.PhoneNo, flightRequest.PhoneNumber },
                                  { Application.Email, flightRequest.ContactEmail },
                                  { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.OrderDataOcpKey) }
                            };

            var bookingDetails = _connector.SendAsync<BookingDetailsResponse>(uri, headers, null, HttpMethod.Get, string.Empty);

            var airportDetails = _referenceDataProvider.GetAirports();

            var airlineDetails = _referenceDataProvider.GetAirlines();

            await Task.WhenAll(bookingDetails, airportDetails, airlineDetails);

            return Mappers.FutureFlightDetailsResponseMapper.MapFlightDetailsResponseMapper(_logger, flightRequest.MemberNumber, bookingDetails.Result.Response, airportDetails.Result, airlineDetails.Result);
        }
    }
}
