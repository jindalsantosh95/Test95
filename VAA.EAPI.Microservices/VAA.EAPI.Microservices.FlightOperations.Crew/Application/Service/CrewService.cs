using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using System.Web.Http;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.FlightOperations.Crew.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Crew.Application.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Crew.Application.Service
{
    public sealed class CrewService(IAzureBlobService azureBlobService, ILogger<CrewService> logger) : ICrewService
    {
        private readonly IAzureBlobService _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        private readonly ILogger<CrewService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly string _storageAccountUrl = ConfigReader.Get("CrewService:AzureBlobService:StorageAccountUrl");
        private readonly string _container = ConfigReader.Get("CrewService:AzureBlobService:Container");
        private readonly string _sasKey = ConfigReader.Get("CrewService:AzureBlobService:SasKey");
        private readonly string _prefix = ConfigReader.Try("CrewService:AzureBlobService:Prefix", "master_crew_");

        public async Task<Result<Response<IEnumerable<CrewMember>>>> RosterAsync()
        {
            var response = new Response<IEnumerable<CrewMember>>()
            {
                Status = Status.OK,
                Value = []
            };

            try
            {
                var blobs = await _azureBlobService.FindBlobsAsync(_storageAccountUrl, _container, _sasKey, _prefix, CancellationToken.None);

                var latestFile = blobs.OrderByDescending(x => x).FirstOrDefault() ?? string.Empty;

                if (!latestFile.IsNullOrWhiteSpace())
                {
                    await _azureBlobService.DownloadBlobAsync(_storageAccountUrl, _container, latestFile, _sasKey, CancellationToken.None);

                    var contents = await _azureBlobService.BlobStringAsync(latestFile, CancellationToken.None);

                    var lines = new List<string[]>();

                    using (TextFieldParser parser = new(new StringReader(contents)))
                    {
                        parser.HasFieldsEnclosedInQuotes = true;

                        parser.SetDelimiters([","]);

                        parser.ReadLine();

                        while (!parser.EndOfData)
                            lines.Add(parser.ReadFields()!);
                    }

                    response.Value = lines.AsParallel()
                                          .Select(x => (CrewMember)x)
                                          .ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Crew roster load failed with {FailureType}.", ex.GetType().Name);
                response.Status = Status.Error;
                response.ErrorMessage = ex.Message;
                response.Errors = [
                    new()
                    {
                        ExceptionType = ex.GetType().Name,
                        Message = ex.Message                         ,
                        DeveloperMessage =  (ex is HttpResponseException exception) ? await exception.Response.Content.ReadAsStringAsync(CancellationToken.None) : null
                    }
                ];
            }

            return Result.Success(response);
        }
    }
}
