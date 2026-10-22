using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Operations.Infrastructure.Entities.AirportInfo_DLJson;
using VAA.EAPI.Microservices.Operations.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Operations.Infrastructure.Services.DLJson
{
    public class AirportInfoProvider_DLJSon : IAirportInfoProvider
    {
        private static readonly SemaphoreSlim sema = new(1, 1);
        private readonly ILogger<IAirportInfoProvider> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IFileSystem _fileSystem;

        public AirportInfoProvider_DLJSon(ILogger<IAirportInfoProvider> logger, IMemoryCache memoryCache, IFileSystem fileSystem)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public async Task<ICollection<Domain.AirportInfo.AirportInfo>> GetAllAirportsAsync()
        {
            ICollection<Domain.AirportInfo.AirportInfo> airportInfoList = new List<Domain.AirportInfo.AirportInfo>();

            await sema.WaitAsync();
            try
            {
                if (!_memoryCache.TryGetValue(Constants.Application.AirportInfoCacheKey, out airportInfoList))
                {
                    _logger.LogInformation("Loading Airport Data from Repo");
                    airportInfoList = new List<Domain.AirportInfo.AirportInfo>();
                    var repoData = Json.Deserialize<ListOfCities>(_fileSystem.File.ReadAllText(System.IO.Path.Combine(VAA.EAPI.Infrastructure.Constants.Constants.FunctionAppDirPath,Constants.Application.AirportListFile)));

                    foreach (var item in repoData.Airports)
                    {
                        Domain.AirportInfo.AirportInfo ai = new()
                        {
                            AirportCode = item.AirportCode,
                            AirportName = item.AirportName,
                            AwardTktTypCd = item.AwardTktTypCd,
                            CityCode = item.CityCode,
                            CityName = item.CityName,
                            CityType = item.CityType,
                            CodeShareOnlyInd = item.CodeShareOnlyInd,
                            CountryCode = item.CountryCode,
                            EurpnUnionCityInd = item.EurpnUnionCityInd,
                            NonStdFreqOfOperationInd = item.NonStdFreqOfOperationInd,
                            NonStdFreqOfOperationText = item.NonStdFreqOfOperationText,
                            Region = item.Region,
                            TimeZone = item.TimeZone,
                            UseTimeForAvailability = item.UseTimeForAvailability
                        };

                        ai.Aliases = new List<string>
                            {
                                $"{ai.CityName}, {ai.Region} ({ai.AirportCode})"
                            };
                        Array.ForEach(item.CityServe.Split('/'), c => ai.Aliases.Add(c));
                        airportInfoList.Add(ai);
                    }
                    _memoryCache.Set(Constants.Application.AirportInfoCacheKey, airportInfoList);
                }
                else
                    _logger.LogInformation("Loading Airport Data from Cache");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Airport Data");
                throw;
            }
            finally
            {
                sema.Release();
            }
            return airportInfoList;
        }
    }
}
