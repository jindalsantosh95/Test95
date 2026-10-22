using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
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
    public sealed class FlightScheduleECommerceService(ILogger<FlightScheduleECommerceService> logger, IHttpConnector httpConnector, IAwsS3Buckets awsS3Buckets) : IFlightScheduleECommerceService
    {
        private bool isWorking = false;
        private readonly ILogger<FlightScheduleECommerceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly IAwsS3BucketService _awsS3BucketService = awsS3Buckets.FirstOrDefault(x => x.Tags.Any(t => t.Is(nameof(FlightScheduleECommerceService)))) ?? throw new ArgumentNullException(nameof(awsS3Buckets));

        public async Task PublishAsync()
        {
            if (isWorking) return;

            isWorking = true;

            try
            {
                var referenceUri = ConfigReader.Get<Uri>("RetailTherapy:ReferenceMs:Uri");
                var flightTypes = ConfigReader.Try("RetailTherapy:FlightScheduleECommerceService:FlightTypes", "J").Split(',').Select(x => x.Trim()).ToArray();
                var fromOffset = ConfigReader.Try("RetailTherapy:FlightScheduleECommerceService:FromOffset", DateTimeSpan.FromMinutes(-30));
                var toOffset = ConfigReader.Try("RetailTherapy:FlightScheduleECommerceService:ToOffset", DateTimeSpan.FromDays(1));
                var paths = ConfigReader.Try("RetailTherapy:FlightScheduleECommerceService:AWS:Folders", "/").Split('|', StringSplitOptions.RemoveEmptyEntries)
                                                                                                             .Select(x => x.Trim('/'))
                                                                                                             .ToArray();

                var flightDetailRequest = new FlightDetailRequest()
                {
                    From = DateTime.UtcNow.Date.Add(fromOffset.Value),
                    To = DateTime.UtcNow.Date.Add(toOffset.Value)
                };

                var headers = new Dictionary<string, string>() { { Constants.Application.OcpKeyHeader, ConfigReader.Get("RetailTherapy:FlightMs:OCPSubsctiption") } };

                var airlinedetails = await _httpConnector.GetAsync<JArray>($"{referenceUri}/airlinedetails");

                var response = await _httpConnector.PostAsync<IEnumerable<FlightDetail>>(ConfigReader.Get("RetailTherapy:FlightMs:DetailsUri"), new StringContent(Common.Serialisation.Json.Serialize(flightDetailRequest)), headers);

                response = [.. response.Where(x => flightTypes.Any(t => t.Is(x.Type)))];

                if (!response.Any()) throw new InvalidOperationException("No data to process.");

                var airportCodes = string.Join(',', response.Select(x => x.Departure).Concat(response.Select(x => x.Arrival)).Distinct());

                var airports = await _httpConnector.GetAsync<JArray>($"{referenceUri}/airportdetail?airportCodes={airportCodes}");

                var airportInfos = airports.Select(x => new
                {
                    Code = x["airportCode"]?.ToString(),
                    TimeZone = x["timeZone"]?.ToString()
                })
                                           .Where(x => !string.IsNullOrEmpty(x.Code))
                                           .ToArray();

                var flightDetails = response.AsParallel()
                                            .Where(flight => airportInfos.Any(a => a.Code.Is(flight.Departure)))
                                            .ToArray();

                var timeZones = flightDetails.AsParallel()
                                             .DistinctBy(x => x.Departure)
                                             .Select(flight =>
                                             {
                                                 var departureAirport = airportInfos.First(a => a.Code.Is(flight.Departure));
                                                 var timeZoneId = departureAirport.TimeZone;

                                                 TimeZoneInfo timeZoneInfo = null;

                                                 try
                                                 {
                                                     timeZoneInfo = TimeZoneInfoHelper.FindSystemTimeZoneById(timeZoneId);
                                                 }
                                                 catch (Exception ex)
                                                 {
                                                     _logger.LogWarning(ex, ex.Message);
                                                 }

                                                 return new KeyValuePair<string, TimeZoneInfo>(timeZoneId, timeZoneInfo);
                                             })
                                            .Where(x => x.Value != null)
                                            .DistinctBy(x => x.Key)
                                            .ToDictionary(x => x.Key, x => x.Value)
                                            .ToArray();

                ConcurrentDictionary<string, TimeZoneInfo> timeZoneCache = new(timeZones);

                var airportByCode = airports.ToDictionary(k => k["airportCode"]?.ToString(), v => new
                {
                    AirportName = v["airportName"]?.ToString() ?? string.Empty,
                    CityName = v["cityName"]?.ToString() ?? string.Empty,
                    CountryName = v["countryName"]?.ToString() ?? string.Empty,
                    TimeZone = v["timeZone"]?.ToString() ?? string.Empty
                }, StringComparer.OrdinalIgnoreCase);

                var carrierByCode = airlinedetails.ToDictionary(k => k["carrierCode"]?.ToString(), v => v["carrierName"]?.ToString() ?? string.Empty, StringComparer.OrdinalIgnoreCase);

                var flights = flightDetails.AsParallel()
                                           .Select(x =>
                                           {
                                               Flight flight = (Flight)x;

                                               if (carrierByCode.TryGetValue(flight.AirlineCode, out var airlineName))
                                                   flight.AirlineName = airlineName;

                                               var departureAirport = airportByCode[flight.Departure];
                                               var arrivalAirport = airportByCode[flight.Arrival];

                                               flight.DepartureAirportName = departureAirport.AirportName;
                                               flight.DepartureCityName = departureAirport.CityName;
                                               flight.DepartureCountryName = departureAirport.CountryName;

                                               flight.ArrivalAirportName = arrivalAirport.AirportName;
                                               flight.ArrivalCityName = arrivalAirport.CityName;
                                               flight.ArrivalCountryName = arrivalAirport.CountryName;

                                               if (!departureAirport.TimeZone.IsNullOrWhiteSpace() && timeZoneCache.TryGetValue(departureAirport.TimeZone, out TimeZoneInfo timeZoneInfo))
                                               {
                                                   try
                                                   {
                                                       var localDeparture = TimeZoneInfo.ConvertTime(flight.ScheduledDateTimeOfDeparture, timeZoneInfo);

                                                       flight.ScheduledDateTimeOfDeparture = localDeparture;
                                                       flight.ScheduledDateOfDeparture = DateOnly.FromDateTime(localDeparture.DateTime);
                                                   }
                                                   catch (ArgumentException ex)
                                                   {
                                                       _logger.LogWarning(ex, "Error converting time from Utc for time zone ID '{TimeZoneId}' for departure airport '{AirportCode}'. Keeping UTC departure time.", timeZoneInfo.Id, flight.Departure);
                                                   }
                                                   catch (InvalidTimeZoneException ex)
                                                   {
                                                       _logger.LogWarning(ex, "Error converting time from Utc for time zone ID '{TimeZoneId}' for departure airport '{AirportCode}'. Keeping UTC departure time.", timeZoneInfo.Id, flight.Departure);
                                                   }
                                               }

                                               return flight;
                                           })
                                           .ToArray();

                var csvHeaders = "primaryFlightNumber,airlineCode,airlineName,scheduledDepartureDate,scheduledDepartureDateTime,scheduledArrivalDate,scheduledArrivalDateTime,arrivalAirportCode,arrivalAirportName,arrivalCountryName,arrivalCityName,departureAirportCode,departureAirportName,departureCountryName,departureCityName,direction";

                var contents = UTF8Encoding.UTF8.GetBytes(flights.ConvertToCsv(csvHeaders));

                foreach (var path in paths)
                {
                    var result = await _awsS3BucketService.UploadAsync("flight.csv", contents, path);

                    if (!result)
                        throw new InvalidOperationException($"Failed to upload file '{path}/flight.csv' to S3 bucket.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error has occurred publishing flight schedule eComms, {ex.Message}");

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
