using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.RefData.Models.AirportInfo;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.RefData.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class AirportInfoQuery
    {
        private const string WordRegexPattern = "(?<!\\w){0}\\w*";
        private const string VirginatlanticWhereWeFly = "https://www.virginatlantic.com/where-we-fly/";
        private const string OneDay = "01:00:00:00";

        private readonly IRefDataService<Infrastructure.Models.ReferenceMS.AirportInfo> _refDataService;
        private readonly ILogger<AirportInfoQuery> _logger;
        private readonly IHttpConnector _httpConnector;

        private string[] _cityNames = [];
        private string[] _airportCodes = [];
        private DateTime _citNamesNextDownlaod = DateTime.MinValue;

        public AirportInfoQuery(IRefDataService<Infrastructure.Models.ReferenceMS.AirportInfo> refDataService, ILogger<AirportInfoQuery> logger, IHttpConnector httpConnector)
        {
            _refDataService = refDataService ?? throw new ArgumentNullException(nameof(refDataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        [UseSorting]
        [UseFiltering]
        [GraphQLName("searchAirportInfo")]
        public async Task<AirportInfoResponse> AirportInfo(string searchTerm, int limit = 0)
        {
            try
            {
                if (_citNamesNextDownlaod < DateTime.Now)
                {
                    var routes = (await _httpConnector.GetAsync<JObject>(ConfigReader.Get("AirportInfoQuery:RoutesApi"), null, string.Empty)).Response;

                    _airportCodes = routes.SelectTokens("$.data.airportList.items[*].airportCode")
                                       .Select(x => x.Value<string>())
                                       .ToArray();

                    _cityNames = routes.SelectTokens("$.data.airportList.items[*].routes[*].destinationGuideUrl")
                                       .Select(x => x.Value<string>())
                                       .Distinct()
                                       .Select(x => (x ?? string.Empty).Replace(VirginatlanticWhereWeFly, string.Empty).Split('/'))
                                       .Where(x => x.Length > 2)
                                       .GroupBy(x => string.Concat(x[0], '/', x[1]), v => v[v.GetUpperBound(0)])
                                       .OrderBy(x => x.Key)
                                       .SelectMany(x => x)
                                       .ToArray();

                    _citNamesNextDownlaod = DateTime.Now.Add(ConfigReader.Try("AirportInfoQuery:FrequencyTimeSpan", TimeSpan.Parse(OneDay)));
                }

                // load
                var airports = (await _refDataService.QueryAs<AirportInfo>()).AsParallel()
                                                                             .Select(x =>
                                                                             {
                                                                                 var cityName = _cityNames.FirstOrDefault(c => x.CityName.Is(c) || (x.CityServe?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? []).Any(a => c.Is(a))) ?? x.CityName;

                                                                                 if (_cityNames.Any(c => cityName.Is(c)) && !x.VaaOperated.HasFlag(VaaOperation.City))
                                                                                     x.VaaOperated |= VaaOperation.City;

                                                                                 if (_airportCodes.Any(c => x.AirportCode.Is(c)) && !x.VaaOperated.HasFlag(VaaOperation.Airport))
                                                                                     x.VaaOperated |= VaaOperation.Airport;

                                                                                 return x;
                                                                             })
                                                                             .ToArray();

                // filter
                airports = airports.AsParallel()
                                   .Where(ai => ai == searchTerm)
                                   .ToArray();
                // order
                airports = airports.OrderByDescending(x =>
                                   {
                                       var pattern = string.Join(@"\s+", searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => string.Format(WordRegexPattern, Regex.Escape(x))));

                                       decimal weight = 0.1M;

                                       if (RegexCompiled.IsMatch(x.CityName.Replace("(", string.Empty).Replace(")", string.Empty), pattern, RegexOptions.IgnoreCase))
                                           x.Weight += weight * 2;

                                       if ((x.CityServe?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? []).Any(x => RegexCompiled.IsMatch(x.Replace("(", string.Empty).Replace(")", string.Empty), pattern, RegexOptions.IgnoreCase)))
                                           x.Weight += weight * 1;

                                       if (RegexCompiled.IsMatch(x.AirportCode, pattern, RegexOptions.IgnoreCase))
                                           x.Weight += weight * 1;

                                       if (RegexCompiled.IsMatch(x.Region, pattern, RegexOptions.IgnoreCase))
                                           x.Weight += weight * 1;

                                       x.Weight = (int)x.VaaOperated + x.Weight;

                                       return x.Weight;
                                   })
                                   .ToArray();

                if (limit > 0)
                    airports = airports.Take(limit)
                                       .ToArray();

                return new AirportInfoResponse()
                {
                    SearchTerm = searchTerm,
                    Count = airports.Length,
                    ListOfCities = airports
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception in AirportInfo Reference Data Query");

                throw;
            }
        }
    }
}
