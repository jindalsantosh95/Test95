using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System.Globalization;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.OperationalSchedule;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Service
{
    public class FlightOperationsService : IFlightOperationsService
    {
        private static readonly Dictionary<string, string> _headers =
          new()
          { {  Constants.Application.OcpKeyHeader, ConfigReader.Try(Constants.Environment.OperationalScheduleOcpKey) } };

        protected readonly IHttpConnector _connector;
        public readonly IAzureADServiceWrapper _azureADServiceWrapper;
        public readonly ILogger<FlightOperationsService> _logger;

        public FlightOperationsService(IHttpConnector connector,  IAzureADServiceWrapper azureADServiceWrapper, ILogger<FlightOperationsService> logger)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _azureADServiceWrapper = azureADServiceWrapper;
            _logger = logger;
        }

        public async Task<Result<Response<OperationalScheduleResponse>>> GetOperationalScheduleAsync(OperationalScheduleRequest request, CancellationToken cancellationToken)
        {
            try
            {

                ArgumentNullException.ThrowIfNull(request, nameof(request));

                var url = ConfigReader.Try<string>(Constants.Environment.OperationalScheduleUrl)
                    .Replace("{Airline_Code}", request.AirlineCode)
                    .Replace("{Local_Date_From}", request.LocalDateFrom.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                    .Replace("{Local_Date_To}", request.LocalDateTo.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                    .Replace("{Origin_Airport_Code}", request.OriginAirportCode)
                    .Replace("{Flight_Number}", request.FlightNumber.ToString());

                // Get a token
                var token = await _azureADServiceWrapper.GetAccessToken(ConfigReader.Get(Constants.Environment.OperationalScheduleAPIClientId), ConfigReader.Get(Constants.Environment.OperationalScheduleAPIClientSecret), ConfigReader.Get(Constants.Environment.OperationalScheduleAPIAuthority), ConfigReader.Get(Constants.Environment.OperationalScheduleAPIResource));

                var response = new Response<OperationalScheduleResponse>() { Status = Status.OK };
                var result = await _connector.GetAsync(url, _headers, token);
                var content = await result.Content.ReadAsStringAsync(cancellationToken);

                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Operational schedule API returned HTTP {StatusCode} for {AirlineCode} flight {FlightNumber} from {LocalDateFrom} to {LocalDateTo} at {OriginAirportCode}.",
                        (int)result.StatusCode,
                        request.AirlineCode,
                        request.FlightNumber,
                        request.LocalDateFrom,
                        request.LocalDateTo,
                        request.OriginAirportCode);
                    return Result.Failure<Response<OperationalScheduleResponse>>(content);
                }

                return Result.Success(new Response<OperationalScheduleResponse>
                {
                    Status = Status.OK,
                    Value = Json.Deserialize<OperationalScheduleResponse>(content)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Operational schedule lookup failed for {AirlineCode} flight {FlightNumber} from {LocalDateFrom} to {LocalDateTo} at {OriginAirportCode}.", request?.AirlineCode, request?.FlightNumber, request?.LocalDateFrom, request?.LocalDateTo, request?.OriginAirportCode);

                throw;
            }
        }
    }
}
