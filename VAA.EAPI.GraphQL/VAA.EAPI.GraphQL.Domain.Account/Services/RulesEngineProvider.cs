using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.SkyMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using AirportDistanceAndCountries = VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS.AirportDistanceAndCountries;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class RulesEngineProvider<T> : IRulesEngineProvider<T>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        private readonly static Dictionary<string, string> headersInRuleMS = new()
        {
            {Application.OcpKeyHeader, ConfigReader.Get(Environment.CommonEAPIOcpAPIMKey)}
        };

        public RulesEngineProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<T> InRulePointsCalculator(CustomerCustMS customerResponse, VSRetroFlownDetail pdmResponse)
        {
            string segmentDistance = (await _connector.GetAsync<string>(ConfigReader.Get(Environment.RefDataMSGenericUrl).Replace("{0}", Application.GetDistanceEndpoint.Replace("{Origin}", pdmResponse.DepartureAirport).Replace("{Destination}", pdmResponse.ArrivalAirport)), null, string.Empty)).Response;

            var pointsResponse = await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.InRuleMSUrl)}{Application.PointsCalculatorEndpoint}", headersInRuleMS, await GetToken(), HttpMethod.Post, Json.Serialize(new AccrualPointsCalculatorRequest
            {
                CustomerDetails = new List<CustomerDetail>()
                {
                     new CustomerDetail
                     {
                          MemberNumber = customerResponse.LoyaltyNumber,
                          TierCode = customerResponse.MemberTier,
                          MemberStatus = customerResponse.Status,
                          MemberType = string.Empty,
                          CurrentTierStartDate = string.Empty,
                          CampaignOrPromotionCode = string.Empty,
                     }
                },
                FlightSegments = new List<FlightSegment>
                {
                    MapFlightSegment(pdmResponse, segmentDistance)
                }

            }));

            return pointsResponse.Response;
        }

        internal static FlightSegment MapFlightSegment(VSRetroFlownDetail pdmResponse, string segmentDistance)
        {
            return new FlightSegment
            {
                OriginAirportCode = pdmResponse.DepartureAirport,
                DestinationAirportCode = pdmResponse.ArrivalAirport,
                SegmentDistance = int.Parse(segmentDistance),
                OperatingCarrier = pdmResponse.OperatingAirlineCode,
                MarketingCarrier = pdmResponse.MarketingAirlineCode,
                OperatingFlightNumber = pdmResponse.OperatingFlightNumber,
                MarketingFlightNumber = pdmResponse.MarketingFlightNumber,
                ScheduledDepartureTime = pdmResponse.DepartureDate == default ? string.Empty : pdmResponse.DepartureDate.ToString(Application.ISODateFormat),
                ClassOfService = pdmResponse.FlownCabinClass ?? string.Empty,
                CabinClass = pdmResponse.FlownCabinClass ?? string.Empty,
                MarketingBookingClass = pdmResponse.VAMarketingBookingClass,
                OperatingBookingClass = pdmResponse.OriginalBookedClass ?? string.Empty,
                FlownClass = !string.IsNullOrEmpty(pdmResponse.FlownUpgradedClass)
                    ? pdmResponse.FlownUpgradedClass
                    : pdmResponse.OriginalBookedClass ?? string.Empty,
                SalesLocation = pdmResponse.SalesLocation ?? string.Empty,
                AgentIataNumber = pdmResponse.IataAgentNumber ?? string.Empty,
                FareBasisCode = pdmResponse.FareBasis ?? string.Empty
            };
        }

        public async Task<T> InRulePointsCalculatorForSkyFlights(CustomerCustMS customerResponse, SkyClaimResponse skyResponse, AirportDistanceAndCountries airportsDistanceAndCountry, string cabinClass)
        {
            var pointsResponse = await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.InRuleMSUrl)}{Application.PointsCalculatorEndpoint}", headersInRuleMS, await GetToken(), HttpMethod.Post, Json.Serialize(new AccrualPointsCalculatorRequest
            {
                FlightSegments = new List<FlightSegment>()
                {
                    new(){
                    OperatingCarrier = skyResponse?.OperatingCarrierCode,
                    MarketingCarrier = skyResponse?.MarketingCarrierCode,
                    MarketingFlightNumber = skyResponse?.MarketingFlightNumber,
                    OperatingFlightNumber = skyResponse?.OperatingFlightNumber,
                    ScheduledDepartureTime = DateTime.ParseExact(skyResponse.ScheduledFlightSegmentDepartureDate, "yyyyMMdd", null).ToString(Application.ISODateFormat),
                    OriginAirportCode = skyResponse?.FlightSegmentOrigin,
                    DestinationAirportCode = skyResponse?.FlightSegmentDestination,
                    OriginCountryCode = airportsDistanceAndCountry?.OriginCountryCode,
                    DestinationCountryCode = airportsDistanceAndCountry?.DestinationCountryCode,
                    ClassOfService = skyResponse?.OperatingCarierBookingClass,
                    CabinClass = cabinClass,
                    OperatingBookingClass = skyResponse?.OperatingCarierBookingClass,
                    MarketingBookingClass = skyResponse?.MarketingCarrierBookingClass,
                    SegmentDistance = int.Parse(airportsDistanceAndCountry?.DistanceInMiles)
                    }
                },

                CustomerDetails = new List<CustomerDetail>()
                {
                    new(){
                    MemberNumber = customerResponse?.LoyaltyNumber,
                            TierCode = customerResponse?.MemberTier,
                            MemberStatus = customerResponse?.Status,
                            MemberType = string.Empty,
                            CurrentTierStartDate = string.Empty,
                            CampaignOrPromotionCode = string.Empty,
                    }
                }
            }));

            return pointsResponse.Response;
        }
        private async Task<string> GetToken()
        {
            return await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.InRuleMSAppID));
        }
    }
}
