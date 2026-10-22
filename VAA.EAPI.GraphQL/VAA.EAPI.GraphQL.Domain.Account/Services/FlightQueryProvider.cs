using Microsoft.Extensions.Logging;
using NodaTime;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Application = VAA.EAPI.GraphQL.Common.Constants.Application;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class FlightQueryProvider : IFlightQueryProvider
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IRefDataService<AirportInfo> _airportInfoService;
        private readonly IRefDataService<AirlineInfo> _airlineInfoService;
        private readonly static Dictionary<string, string> msAuthToken = new();
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly ILogger<FlightQueryProvider> _logger;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _customerTransFlyingClubMember;


        public FlightQueryProvider(ILogger<FlightQueryProvider> logger, IHttpConnector httpConnector, IRefDataService<AirportInfo> airportInfoService, IRefDataService<AirlineInfo> airlineInfoService, IOAuthTokenProvider tokenProvider, IAccountCustomerService customerProvider, ICustomerTransactionQueryProvider<FcMemberDetails> customerTransFlyingClubMember)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _airportInfoService = airportInfoService ?? throw new ArgumentNullException(nameof(airportInfoService));
            _airlineInfoService = airlineInfoService ?? throw new ArgumentNullException(nameof(airlineInfoService));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerTransFlyingClubMember = customerTransFlyingClubMember ?? throw new ArgumentNullException(nameof(customerTransFlyingClubMember));
        }

        public async Task<FlightDetailsResponse> GetFlightsQueryData(string memberNumber, int page, int pageSize)
        {
            var customerResponse = await _customerProvider.GetCustomer(null, memberNumber);

            var customerFcmDetails = await _customerTransFlyingClubMember.CustomerTransactionQuery($"{ConfigReader.Get(Environment.CustTransactionMsUrl)}{Application.GetFcMemberDetails}", customerResponse.Customer.CustomerId);

            if (customerFcmDetails.FlyingClubMemberDocuments.First().TierCode.Is(Application.VirginAccountTierCode) && ConfigReader.Try(Environment.EnableMockResponseForVACLoginOverview, false))
                return MockFlightResponse();

            if (ConfigReader.Try(Environment.EnableOrderSearchFlightService, false))
            {
                var airportDetails = await _airportInfoService.Query();
                var airlineDetails = await _airlineInfoService.Query();

                APIResponse<OrderBookingResponse> flightBookingDetailsbyEmail = new();
                APIResponse<OrderBookingResponse> flightBookingDetailsbyFcNumber = new();

                if (!customerFcmDetails.FlyingClubMemberDocuments.First().TierCode.Is(Application.VirginAccountTierCode))
                    flightBookingDetailsbyFcNumber = await _httpConnector.SendAsync<OrderBookingResponse>(ConfigReader.Get(Environment.OrderFutureBookingUrl), getHeaders(memberNumber, string.Empty, string.Empty, page.ToString(), pageSize.ToString(), ConfigReader.Get(Environment.OrderDataOcpKey)), await GetToken(ConfigReader.Get(Environment.OrderDataAppId)), HttpMethod.Get, string.Empty);

                flightBookingDetailsbyEmail = await _httpConnector.SendAsync<OrderBookingResponse>(ConfigReader.Get(Environment.OrderFutureBookingUrl), getHeaders(string.Empty, string.Empty, customerResponse.Customer.ContactDetails.EmailDetails.First(x => x.PreferredFlag == "Y").emailId, page.ToString(), pageSize.ToString(), ConfigReader.Get(Environment.OrderDataOcpKey)), await GetToken(ConfigReader.Get(Environment.OrderDataAppId)), HttpMethod.Get, string.Empty);

                return (FlightDetailsResponse)(flightBookingDetailsbyEmail.Response, flightBookingDetailsbyFcNumber.Response, airportDetails, airlineDetails, customerFcmDetails.FlyingClubMemberDocuments.First().TierCode);
            }

            else
            {
                var bookingDetails = await _httpConnector.SendAsync<BookingDetailsResponse.BookingResponse>(ConfigReader.Get(Common.Constants.Environment.FutureBookingUrl), getHeaders(memberNumber, string.Empty, string.Empty, page.ToString(), pageSize.ToString(), ConfigReader.Get(Common.Constants.Environment.OrderSearchOcpKey)), await GetToken(ConfigReader.Get(Common.Constants.Environment.OrderSearchAppId)), HttpMethod.Get, string.Empty);
                return await FlightDetailsResponseMapper.MapFlightDetailsResponseMapper(_logger, memberNumber, bookingDetails.Response, _airportInfoService, _airlineInfoService);
            }
        }

        private async Task<string> GetToken(string appId)
        {
            return (msAuthToken.ContainsKey(appId) && msAuthToken[appId].IsJwtTokenValid()) ? msAuthToken[appId] : msAuthToken[appId] = await _tokenProvider.AcquireTokenAsync(appId);
        }

        private static Dictionary<string, string> getHeaders(string memberNumber, string phoneNo, string email, string page, string pageSize, string OcpKey)
        {
            return new Dictionary<string, string>
                {
                    { Application.FcNumber, memberNumber },
                    { Application.PhoneNo, phoneNo },
                    { Application.Email, email },
                    { Application.OcpKeyHeader, OcpKey },
                    { Application.Page, page },
                    { Application.PageSize, pageSize }
                };
        }
        private static FlightDetailsResponse MockFlightResponse()
        {
            return new FlightDetailsResponse
            {
                FlightSegmentDetails = new List<FlightSegmentDetails>()
                {
                    new()
                    {
                        OperatingAirlineName = "Virgin Atlantic",
                        OriginAirportName = "Heathrow Airport",
                        FlightNumber = "VS105",
                        BookingReference = "EA3X7K",
                        DateOfFlight = LocalDate.FromDateTime(DateTime.UtcNow.AddDays(50)),
                        DestinationAirportCode = "MIA",
                        DestinationAirportCountry = "FL",
                        DestinationAirportName = "Miami International Airport",
                        NumberOfDaysToDeparture = 50,
                        OperatingAirlineCode = "VS",
                        OriginAirportCountry = "United Kingdom",
                        OriginAirportCode = "LHR",
                        ScheduledDepartureTime = Common.Helper.DateTimeHelper.ParseVaaBookingTime("0145A").Value,
                        ShowClaimsBanner = true,
                        VirginPoints = "7100",
                        TierPoints = "50"
                    }
                }
            };
        }

    }
}
