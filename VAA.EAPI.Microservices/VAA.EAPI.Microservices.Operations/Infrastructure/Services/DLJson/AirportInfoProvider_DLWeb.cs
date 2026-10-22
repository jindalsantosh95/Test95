using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Operations.Infrastructure.Entities.AirportInfo_DLJson;
using VAA.EAPI.Microservices.Operations.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Operations.Infrastructure.Services
{
    public class AirportInfoProvider_DLWeb : IAirportInfoProvider
    {
        private static readonly SemaphoreSlim sema = new(1, 1);
        private readonly ILogger<IAirportInfoProvider> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpConnector _apiConnector;

        public AirportInfoProvider_DLWeb(ILogger<IAirportInfoProvider> logger, IMemoryCache memoryCache, IHttpConnector apiConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }


        public async Task<ICollection<Domain.AirportInfo.AirportInfo>> GetAllAirportsAsync()
        {
            ICollection<Domain.AirportInfo.AirportInfo> airportInfoList = new List<Domain.AirportInfo.AirportInfo>();

            await sema.WaitAsync();
            try
            {
                if (!_memoryCache.TryGetValue(Constants.Application.AirportInfoCacheKey, out airportInfoList))
                {
                    airportInfoList = await LoadDataFromWebSite();
                    _memoryCache.Set(Constants.Application.AirportInfoCacheKey, airportInfoList, DateTime.UtcNow.Add(ConfigReader.Get<TimeSpan>(Constants.Environment.AirportInfoCache)));
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

        private async Task<ICollection<Domain.AirportInfo.AirportInfo>> LoadDataFromWebSite()
        {
            #region Get the data from the website
            _logger.LogInformation("Loading Airport Data from Data Source");
            ListOfCities apiResponse = new();


            var headers = new Dictionary<string, string>()
            {
                { "accept", ConfigReader.Get(Constants.Environment.AirportInfoAccept)},
                { "user-agent", ConfigReader.Get(Constants.Environment.AirportInfoAgent)}
            };

            string url = ConfigReader.Get(Constants.Environment.AirportInfoURL);
            try
            {
                apiResponse = await _apiConnector.GetAsync<ListOfCities>(url, headers, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Retrieving airport data from the web source failed.");
                throw;
            }
            #endregion

            #region Convert the data
            var airportInfoList = new List<Domain.AirportInfo.AirportInfo>();
            foreach (var item in apiResponse.Airports)
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
            #endregion

            return airportInfoList;
        }
    }
}
