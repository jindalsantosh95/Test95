using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services.Aws;
using VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Interfaces;
using VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Models.FlightMs;

namespace VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Services
{
    public sealed class FlightScheduleConsolidatedService(ILogger<FlightScheduleConsolidatedService> logger, IHttpConnector httpConnector, IAwsS3Buckets awsS3Buckets) : IFlightScheduleConsolidatedService
    {
        private bool isWorking = false;
        private readonly ILogger<FlightScheduleConsolidatedService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly IAwsS3BucketService _awsS3BucketService = awsS3Buckets.FirstOrDefault(x => x.Tags.Any(t => t.Is(nameof(FlightScheduleConsolidatedService)))) ?? throw new ArgumentNullException(nameof(awsS3Buckets));

        public async Task PublishAsync()
        {
            if (isWorking) return;

            isWorking = true;

            try
            {
                var flightTypes = ConfigReader.Try("RetailTherapy:FlightScheduleConsolidatedService:FlightTypes", "J").Split(',').Select(x => x.Trim()).ToArray();
                var fromOffset = ConfigReader.Try("RetailTherapy:FlightScheduleConsolidatedService:FromOffset", DateTimeSpan.FromMinutes(-30));
                var toOffset = ConfigReader.Try("RetailTherapy:FlightScheduleConsolidatedService:ToOffset", DateTimeSpan.FromDays(1));
                var paths = ConfigReader.Try("RetailTherapy:FlightScheduleConsolidatedService:AWS:Folders", "/").Split('|', StringSplitOptions.RemoveEmptyEntries)
                                                                                                                .Select(x => x.Trim('/'))
                                                                                                                .ToArray();

                var flightDetailRequest = new FlightDetailRequest()
                {
                    From = DateTime.UtcNow.Date.Add(fromOffset.Value),
                    To = DateTime.UtcNow.Date.Add(toOffset.Value)
                };

                var headers = new Dictionary<string, string>() { { Constants.Application.OcpKeyHeader, ConfigReader.Get("RetailTherapy:FlightMs:OCPSubsctiption") } };

                var response = await _httpConnector.PostAsync<IEnumerable<FlightDetail>>(ConfigReader.Get("RetailTherapy:FlightMs:DetailsUri"), new StringContent(Common.Serialisation.Json.Serialize(flightDetailRequest)), headers);

                response = [.. response.Where(x => flightTypes.Any(t => t.Is(x.Type)))];

                if (!response.Any()) throw new InvalidOperationException("No data to process.");

                var csvHeaders = "LegID;AirlineCode;FlightNumber;ScheduledOriginAirportCode;ScheduledDestinationAirportCode;ScheduledDepartureDateTime;ScheduledArrivalDateTime;AircraftType;AircraftRegistration;AircraftCapacity;LinkedFlightNumber";

                var key = $"Schedule_{flightDetailRequest.From:yyyyMMdd}_{flightDetailRequest.To:yyyyMMdd}.csv";

                var contents = UTF8Encoding.UTF8.GetBytes(response.ConvertToCsv(csvHeaders));

                foreach (var path in paths)
                {
                    var result = await _awsS3BucketService.UploadAsync(key, contents, path);

                    if (!result)
                        throw new InvalidOperationException($"Failed to upload file '{path}/{key}'to S3 bucket.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error has occurred publishing flight schedule consolidated, {ex.Message}");

                // we throw the exception back as we are using the timer function retry capability
                throw;
            }
            finally
            {
                isWorking = false;
            }
        }
    }
}
