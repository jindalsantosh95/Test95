using CSharpFunctionalExtensions;
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
    public sealed class FlightCrewRosterService(ILogger<FlightCrewRosterService> logger, IHttpConnector httpConnector, IAwsS3Buckets awsS3Buckets) : IFlightCrewRosterService
    {
        private const string RoleTypeCC = "CC";
        private const string RoleTypeCSS = "CSS";
        private const string RoleTypeFSM = "FSM";

        private bool isWorking = false;
        private readonly ILogger<FlightCrewRosterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly IAwsS3BucketService _awsS3BucketService = awsS3Buckets.FirstOrDefault(x => x.Tags.Any(t => t.Is(nameof(FlightCrewRosterService)))) ?? throw new ArgumentNullException(nameof(awsS3Buckets));

        public async Task PublishAsync()
        {
            if (isWorking) return;

            isWorking = true;

            try
            {
                var flightTypes = ConfigReader.Try("RetailTherapy:FlightCrewRosterService:FlightTypes", "J").Split(',').Select(x => x.Trim()).ToArray();
                var fromOffset = ConfigReader.Try("RetailTherapy:FlightCrewRosterService:FromOffset", DateTimeSpan.FromMinutes(-30));
                var toOffset = ConfigReader.Try("RetailTherapy:FlightCrewRosterService:ToOffset", DateTimeSpan.FromDays(1));
                var paths = ConfigReader.Try("RetailTherapy:FlightCrewRosterService:AWS:Folders", "/").Split('|', StringSplitOptions.RemoveEmptyEntries)
                                                                                                      .Select(x => x.Trim('/'))
                                                                                                      .ToArray();

                var flightDetailRequest = new FlightDetailRequest()
                {
                    From = DateTime.UtcNow.Date.Add(fromOffset.Value),
                    To = DateTime.UtcNow.Date.Add(toOffset.Value)
                };

                var flightMsHeaders = new Dictionary<string, string>() { { Constants.Application.OcpKeyHeader, ConfigReader.Get("RetailTherapy:FlightMs:OCPSubsctiption") } };

                var response = await _httpConnector.PostAsync<IEnumerable<FlightDetail>>(ConfigReader.Get("RetailTherapy:FlightMs:DetailsUri"), new StringContent(Common.Serialisation.Json.Serialize(flightDetailRequest)), flightMsHeaders);

                response = [.. response.Where(x => flightTypes.Any(t => t.Is(x.Type)))];

                var lines = new List<dynamic>();

                foreach (var chunk in response.Chunk(10))
                {
                    var records = chunk.Select(async x =>
                                        {
                                            var crewRosterRequest = new FlightCrewRosterRequest()
                                            {
                                                Departure = x.Departure,
                                                FlightDate = x.Date,
                                                FlightNo = x.FlightNo
                                            };

                                            var crewRosterResponse = await _httpConnector.PostAsync(ConfigReader.Get("RetailTherapy:FlightMs:CrewRosterUri"), new StringContent(Common.Serialisation.Json.Serialize(crewRosterRequest)), flightMsHeaders);

                                            var crew = crewRosterResponse.IsSuccessStatusCode ? await crewRosterResponse.Content.ReadAsAsync<IEnumerable<FlightCrewMember>>() : [];

                                            return new
                                            {
                                                FlightId = $"{x.Date:yyyyMMdd}{x.ScheduledTimeOfDeparture:HHmm}{x.FlightNo}",
                                                FlightNumber = x.FlightNo,
                                                FlightDate = x.Date,
                                                Crew = crew.Where(x => x.Pos.IsAny(RoleTypeCC, RoleTypeCSS, RoleTypeFSM)).ToArray()
                                            };
                                        })
                                        .ToArray();

                    await Task.WhenAll(records);

                    var batch = records.Where(x => x.Result.Crew.Length > 0)
                                       .Select(x => AppendFlightDetails(x.Result));

                    if (batch.Any())
                        lines.AddRange(batch);
                }

                if (!lines.Any()) throw new InvalidOperationException("No data to process.");

                var csvHeaders = "FLIGHT_ID;FLIGHT_NUMBER;DEPARTURE_DATE;CREW_ID;CREW_NAME;CREW_ROLE";

                lines = [.. (new string[] { csvHeaders }), .. lines];

                var key = $"CrewRoster_{flightDetailRequest.From:yyyyMMdd}_{flightDetailRequest.To:yyyyMMdd}.csv";

                var contents = UTF8Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines));

                foreach (var path in paths)
                {
                    var result = await _awsS3BucketService.UploadAsync(key, contents, path);

                    if (!result)
                        throw new InvalidOperationException($"Failed to upload file '{path}/{key}'to S3 bucket.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error has occurred publishing crew roster, {ex.Message}");

                // we throw the exception back as we are using the timer function retry capability
                throw;
            }
            finally
            {
                isWorking = false;
            }
        }

        private static string AppendFlightDetails(dynamic arg)
        {
            var lines = ((IEnumerable<FlightCrewMember>)arg.Crew).ConvertToCsv()
                                                                 .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            lines = [.. lines.Select(x => $"{arg.FlightId};{arg.FlightNumber};{arg.FlightDate:yyyy-MM-dd};{x}")];

            return string.Join(Environment.NewLine, lines);
        }
    }
}
