using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services.Aws;
using VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Interfaces;
using VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Models.FlightMs;

namespace VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Services
{
    public sealed class CrewMasterService(ILogger<CrewMasterService> logger, IHttpConnector httpConnector, IAwsS3Buckets awsS3Buckets) : ICrewMasterService
    {
        private bool isWorking = false;
        private readonly ILogger<CrewMasterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly IAwsS3BucketService _awsS3BucketService = awsS3Buckets.FirstOrDefault(x => x.Tags.Any(t => t.Is(nameof(CrewMasterService)))) ?? throw new ArgumentNullException(nameof(awsS3Buckets));

        public async Task PublishAsync()
        {
            if (isWorking) return;

            isWorking = true;

            try
            {
                var paths = ConfigReader.Try("RetailTherapy:CrewMasterService:AWS:Folders", "/").Split('|', StringSplitOptions.RemoveEmptyEntries)
                                                                                                .Select(x => x.Trim('/'))
                                                                                                .ToArray();
                var headers = new Dictionary<string, string>() { { Constants.Application.OcpKeyHeader, ConfigReader.Get("RetailTherapy:CrewMs:OCPSubsctiption") } };

                var response = await _httpConnector.GetAsync<IEnumerable<CrewMember>>(ConfigReader.Get("RetailTherapy:CrewMs:Uri"), headers);

                if (!response.Any()) throw new InvalidOperationException("No data to process.");

                var csvHeaders = "CrewID;CrewFirstName;CrewLastName;CrewMailAddress;CrewBase";

                var key = $"Crew_{DateTime.UtcNow:yyyyMMdd}.csv";

                var contents = UTF8Encoding.UTF8.GetBytes(response.ConvertToCsv(csvHeaders));

                foreach (var path in paths)
                {
                    var result = await _awsS3BucketService.UploadAsync(key, contents, path);

                    if (!result)
                        throw new InvalidOperationException($"Failed to upload file '{path}/{key}' to S3 bucket.");
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
    }
}
