using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models.Retro;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.CreateTransaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.SkyMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using AirportDistanceAndCountries = VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS.AirportDistanceAndCountries;
using Application = VAA.EAPI.GraphQL.Common.Constants.Application;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class ClaimPartnerFlightsPointsMutation
    {
        private readonly ILogger<ClaimPartnerFlightsPointsMutation> _logger;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ISkyRetroProvider<CheckEligibilityClaimResponse> _skyProviderEligibility;
        private readonly ISkyRetroProvider<SkyFeedbackResponse> _skyProviderFeedback;
        private readonly IRefDataService<ProductDetails> _productRefService;
        private readonly IRefDataService<PartnerProfileDetails> _partnerProfileService;
        private readonly IRefDataService<AirportInfo> _airportInfoService;
        private readonly IRefDataService<AirlineInfo> _airlineInfoService;
        private readonly IRulesEngineProvider<AccrualPointsCalculatorResponse> _ruleEngineProvider;
        private readonly ILoyaltyMemberManagementProvider _loyaltyMemberManagementProvider;
        private readonly ITransactionMutationProvider _transactionMutationProvider;
        private readonly ICustomerTransactionQueryProvider<FlightTransactionResponse> _customerTransactionQueryProvider;
        private readonly ICustomerTransactionQueryProvider<TransactionResponse<Transaction>> _customerTransactionDocProvider;

        public ClaimPartnerFlightsPointsMutation(ILogger<ClaimPartnerFlightsPointsMutation> logger, ITokenB2CProvider<string> tokenB2CProvider, IHttpConnector httpConnector, IAccountCustomerService customerProvider, ISkyRetroProvider<CheckEligibilityClaimResponse> skyProviderEligibility, ISkyRetroProvider<SkyFeedbackResponse> skyProviderFeedback, IRefDataService<ProductDetails> productRefService, IRefDataService<PartnerProfileDetails> partnerProfileService, IRefDataService<AirportInfo> airportInfoService, IRefDataService<AirlineInfo> airportlineService, IRulesEngineProvider<AccrualPointsCalculatorResponse> ruleEngineProvider, ILoyaltyMemberManagementProvider loyaltyMemberManagementProvider, ITransactionMutationProvider transactionMutationProvider, ICustomerTransactionQueryProvider<FlightTransactionResponse> customerTransactionQueryProvider, ICustomerTransactionQueryProvider<TransactionResponse<Transaction>> customerTransactionDocProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _skyProviderEligibility = skyProviderEligibility ?? throw new ArgumentNullException(nameof(skyProviderEligibility));
            _skyProviderFeedback = skyProviderFeedback ?? throw new ArgumentNullException(nameof(skyProviderFeedback));
            _productRefService = productRefService ?? throw new ArgumentNullException(nameof(productRefService));
            _partnerProfileService = partnerProfileService ?? throw new ArgumentNullException(nameof(partnerProfileService));
            _airportInfoService = airportInfoService ?? throw new ArgumentNullException(nameof(airportInfoService));
            _airlineInfoService = airportlineService ?? throw new ArgumentNullException(nameof(airportlineService));
            _ruleEngineProvider = ruleEngineProvider ?? throw new ArgumentNullException(nameof(ruleEngineProvider));
            _loyaltyMemberManagementProvider = loyaltyMemberManagementProvider ?? throw new ArgumentNullException(nameof(loyaltyMemberManagementProvider));
            _transactionMutationProvider = transactionMutationProvider ?? throw new ArgumentNullException(nameof(transactionMutationProvider));
            _customerTransactionQueryProvider = customerTransactionQueryProvider ?? throw new ArgumentNullException(nameof(customerTransactionQueryProvider));
            _customerTransactionDocProvider = customerTransactionDocProvider ?? throw new ArgumentNullException(nameof(customerTransactionQueryProvider));
        }

        [GraphQLName("claimPartnerFlightPoints")]
        [GraphQLDescription("Claim Missing points for Partner Airlines")]
        public async Task<MissingPointsResponse<ClaimFlightSegmentDetails>> ClaimMissingPointsSky(SkyFlightsPointsClaimRequest request)
        {
            if (request.TicketNumber.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request));
            if (request.FcNumber.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request));
            if (request.DepartureDate.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request));
            if (request.DepartureFrom.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request));
            if (request.ArrivingAt.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request));

            if (ConfigReader.Try<bool>(Environment.EnableMockResponseForPartnersFlightsPointsClaim))
            {
                return GetMockSkyClaimPointResponse();
            }

            var loginToken = AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>().AccessToken.Token;
            ArgumentNullException.ThrowIfNull(loginToken);

            try
            {
                var claim = Json.Deserialize<TokenB2CResponse>(await _tokenB2CProvider.ValidateTokenAsync(loginToken));

                IEnumerable<ProductDetails> productDetails = null;
                IEnumerable<PartnerProfileDetails> partnerProfileDetails = null;
                IEnumerable<AirportInfo> airportDetails = null;
                IEnumerable<AirlineInfo> airlineDetails = null;
                AirportDistanceAndCountries airportsDistanceAndCountries = new();
                CabinClassResponse cabinDetails = new();

                var customerResponse = await _customerProvider.GetCustomer(null, request.FcNumber);

                var skyTeamFlightResponse = await _skyProviderEligibility.CheckClaimEligibility(customerResponse, request);

                Parallel.Invoke(
                    new Action(() => productDetails = _productRefService.Query().GetAwaiter().GetResult()),
                    new Action(() => partnerProfileDetails = _partnerProfileService.Query().GetAwaiter().GetResult()),
                    new Action(() => airportDetails = _airportInfoService.Query().GetAwaiter().GetResult()),
                    new Action(() => airlineDetails = _airlineInfoService.Query().GetAwaiter().GetResult()),
                    new Action(() => cabinDetails = _httpConnector.SendAsync<CabinClassResponse>(ConfigReader.Get(Environment.RefDataMSGenericUrl).Replace("{0}", Application.RefMsGetCabinClassDetailsUrl), null, string.Empty, HttpMethod.Post, Json.Serialize(MissingPointsMapper.MapToGetCabinClassDetailsRequest(request, skyTeamFlightResponse.SkyClaimResponse))).GetAwaiter().GetResult().Response),
                    new Action(() => airportsDistanceAndCountries = _httpConnector.GetAsync<AirportDistanceAndCountries>(ConfigReader.Get(Environment.RefDataMSGenericUrl).Replace("{0}", Application.RefMsGetSegmentDistanceAndCountryUrl + request.DepartureFrom + "/" + request.ArrivingAt), null, string.Empty).GetAwaiter().GetResult().Response)
                    );

                var inRuleResponse = await _ruleEngineProvider.InRulePointsCalculatorForSkyFlights(customerResponse.Customer, skyTeamFlightResponse.SkyClaimResponse, airportsDistanceAndCountries, cabinDetails.CabinClass);

                var flighProductDetails = productDetails.First(x => string.Equals(x.ProductCategory, Application.ProductCategory, StringComparison.OrdinalIgnoreCase));

                var flightPartnerDetails = partnerProfileDetails.First(x => string.Equals(x.PartnerCode, skyTeamFlightResponse.SkyClaimResponse.OperatingCarrierCode, StringComparison.OrdinalIgnoreCase));

                var inRuleSegmentResponse = inRuleResponse.SegmentResponses.First();

                var transactionRequest = MissingPointsMapper.MapTransactionReqforSkyFlights(customerResponse.Customer, inRuleSegmentResponse, skyTeamFlightResponse.SkyClaimResponse, flightPartnerDetails, flighProductDetails.RowId, airportsDistanceAndCountries.DistanceInMiles, request, cabinDetails.CabinClass);

                //Checking for existing transactions for customer 
                var duplicateTransaction = _customerTransactionDocProvider.CustomerTransactionQuery($"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetTransactionEndPoint}".Replace("{CustomerId}", transactionRequest.TransactionDocument.CustomerId), transactionRequest.TransactionDocument.CustomerId).Result.transactionDocuments.Any(n => n.ProductId == transactionRequest.TransactionDocument.ProductId && n.StatusCode == transactionRequest.TransactionDocument.StatusCode && Convert.ToDateTime(n.ActivityDate).Date == Convert.ToDateTime(transactionRequest.TransactionDocument.ActivityDate).Date && n.FlightDetails.OriginAirportcode == transactionRequest.TransactionDocument.FlightDetails.OriginAirportcode && n.FlightDetails.DestinationAirport == transactionRequest.TransactionDocument.FlightDetails.DestinationAirport && n.TransactionType == transactionRequest.TransactionDocument.TransactionType);

                if (duplicateTransaction)
                    throw ClaimExceptionResponse(Constants.Application.VredCreateTransactionErrorCode, "Duplicate Transaction found", "Duplicate Transaction found");

                var statementText = await _loyaltyMemberManagementProvider.GenerateStatementText(MissingPointsMapper.GetStatementTextForSkyFlights(transactionRequest.TransactionDocument, request, flighProductDetails, flightPartnerDetails, skyTeamFlightResponse.SkyClaimResponse, cabinDetails.CabinClass));
                transactionRequest.TransactionDocument.StatementText = statementText.StatementText;

                var vredTransactionRequest = MissingPointsMapper.CreateTransactionSkyRetroVRedMapper(transactionRequest.TransactionDocument, flighProductDetails, flightPartnerDetails, statementText);

                var TransactionDetails = await _customerTransactionQueryProvider.CreateTransaction(transactionRequest);


                var createVredTransaction = await _transactionMutationProvider.CreateVredFlightTransaction(vredTransactionRequest);

                if (createVredTransaction.Status == Common.Entities.Enums.Status.OK)
                {
                    var skyTeamFeedbackResponse = await _skyProviderFeedback.FeedBackUpdate(skyTeamFlightResponse, inRuleSegmentResponse);
                }
                else
                {
                    throw ClaimExceptionResponse(Constants.Application.VredCreateTransactionErrorCode, "Error in VRed MS while updating data", "Error in VRed MS while updating data");
                }

                return MissingPointsMapper.GetSkyMissingPointsClaimResponse(skyTeamFlightResponse.SkyClaimResponse, airlineDetails.First(x => x.CarrierCode == skyTeamFlightResponse.SkyClaimResponse.OperatingCarrierCode).CarrierName, inRuleResponse.SegmentResponses.FirstOrDefault(), airportDetails.First(x => string.Equals(x.AirportCode, skyTeamFlightResponse.SkyClaimResponse.FlightSegmentOrigin, StringComparison.OrdinalIgnoreCase)), airportDetails.First(x => string.Equals(x.AirportCode, skyTeamFlightResponse.SkyClaimResponse.FlightSegmentDestination, StringComparison.OrdinalIgnoreCase)));
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Partner Airlines Points Claim failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty())
                {
                    var builder = ErrorBuilder.New().SetMessage("Partner Airlines Points Claim failed");
                    foreach (var error in errorDetails)
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    }
                    ;

                    throw new GraphQLException(builder.Build());
                }

                throw ClaimExceptionResponse(Constants.Application.MissingPointsErrorCode, $"Error While processing Sky partners flight Points Claim\r\n{ex.Message}", ex.Message);
            }

            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception while claiming missing poitns for Partner Airlines Flights \r\n{ex.Message}");
                throw ClaimExceptionResponse(Constants.Application.MissingPointsErrorCode, "Error While processing Sky partners flight Points Claim", ex.Message);
            }
        }

        private static GraphQLException ClaimExceptionResponse(string code, string message, string exception)
        {
            return new GraphQLException(ErrorBuilder.New()
                            .SetMessage(message)
                            .SetExtension("Exception", exception)
                            .SetCode(code)
                            .Build());
        }

        private static MissingPointsResponse<ClaimFlightSegmentDetails> GetMockSkyClaimPointResponse()
        {
            var segmentDetails = new ClaimFlightSegmentDetails
            {
                OperatingAirlineName = "Air France",
                OperatingAirlineCode = "AF",
                FlightNumber = "AF2631",
                DateOfFlight = "2024-08-19T12:00:00",
                OriginAirportCode = "LHR",
                OriginAirportName = "Heathrow Airport",
                OriginAirportCountry = "United Kingdom",
                DestinationAirportCode = "SFO",
                DestinationAirportName = "San Francisco International",
                DestinationAirportCountry = "CA",
                VirginPoints = "2350",
                BonusPoints = "1000",
                TierPoints = "50",
            };

            return new MissingPointsResponse<ClaimFlightSegmentDetails>
            {
                TotalBonusPoints = "100",
                TotalTierPoints = "200",
                TotalVirginPoints = "3350",
                segmentDetails = new[] { segmentDetails },
                ErrorResponse = null
            };
        }
    }
}