using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Helpers;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Retro;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.CreateTransaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using AccountApplication = VAA.EAPI.GraphQL.Domain.Account.Constants.Application;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class ClaimVSPointsMutation
    {
        private readonly ILogger<ClaimVSPointsMutation> _logger;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly IAccountCustomerService _customerProvider;
        private readonly IPDMProvider _pdmProvider;
        private readonly IRefDataService<ProductDetails> _productRefService;
        private readonly IRefDataService<PartnerProfileDetails> _partnerProfileService;
        private readonly IRefDataService<AirportInfo> _airportInfoService;
        private readonly IRulesEngineProvider<AccrualPointsCalculatorResponse> _inRuleProvider;
        private readonly ILoyaltyMemberManagementProvider _loyaltyMemberManagementProvider;
        private readonly ITransactionMutationProvider _transactionMutationProvider;
        private readonly ICustomerTransactionQueryProvider<FlightTransactionResponse> _customerTransactionQueryProvider;
        private readonly ICustomerTransactionQueryProvider<TransactionResponse<Transaction>> _customerTransactionDocProvider;
        private readonly TimeProvider _timeProvider;

        public ClaimVSPointsMutation(ILogger<ClaimVSPointsMutation> logger, ITokenB2CProvider<string> tokenB2CProvider, IHttpConnector httpConnector, IAccountCustomerService customerProvider, IPDMProvider pdmProvider, IRefDataService<ProductDetails> productRefService, IRefDataService<PartnerProfileDetails> partnerProfileService, IRefDataService<AirportInfo> airportInfoService, IRulesEngineProvider<AccrualPointsCalculatorResponse> inRuleProvider, ILoyaltyMemberManagementProvider loyaltyMemberManagementProvider, ITransactionMutationProvider transactionMutationProvider, ICustomerTransactionQueryProvider<FlightTransactionResponse> customerTransactionQueryProvider, ICustomerTransactionQueryProvider<TransactionResponse<Transaction>> customerTransactionDocProvider, TimeProvider timeProvider = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _pdmProvider = pdmProvider ?? throw new ArgumentNullException(nameof(pdmProvider));
            _productRefService = productRefService ?? throw new ArgumentNullException(nameof(productRefService));
            _partnerProfileService = partnerProfileService ?? throw new ArgumentNullException(nameof(partnerProfileService));
            _airportInfoService = airportInfoService ?? throw new ArgumentNullException(nameof(airportInfoService));
            _inRuleProvider = inRuleProvider ?? throw new ArgumentNullException(nameof(inRuleProvider));
            _loyaltyMemberManagementProvider = loyaltyMemberManagementProvider ?? throw new ArgumentNullException(nameof(loyaltyMemberManagementProvider));
            _transactionMutationProvider = transactionMutationProvider ?? throw new ArgumentNullException(nameof(transactionMutationProvider));
            _customerTransactionQueryProvider = customerTransactionQueryProvider ?? throw new ArgumentNullException(nameof(customerTransactionQueryProvider));
            _customerTransactionDocProvider = customerTransactionDocProvider ?? throw new ArgumentNullException(nameof(customerTransactionQueryProvider));
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        [GraphQLName("claimVSPoints")]
        [GraphQLDescription("Claim VS missing points")]
        public async Task<MissingPointsResponse<ClaimFlightSegmentDetails>> claimPoints(string ticketNumber, string fcNumber)
        {
            if (ticketNumber.IsNullOrEmpty()) throw new ArgumentNullException(nameof(ticketNumber));

            if (fcNumber.IsNullOrEmpty()) throw new ArgumentNullException(nameof(fcNumber));


            if (ConfigReader.Try<bool>(Environment.EnableMockResponseForVSPointsClaim, false))
            {
                _logger.LogInformation("Returning Mock Response");
                return GetMockClaimPointResponse();
            }

            var loginToken = AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>().AccessToken.Token;
            ArgumentNullException.ThrowIfNull(loginToken);

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(loginToken);

                CustomerDTO customerResponse = null;
                IEnumerable<ProductDetails> productDetails = null;
                IEnumerable<PartnerProfileDetails> partnerProfileDetails = null;
                IEnumerable<AirportInfo> airportDetails = null;
                List<ClaimFlightSegmentDetails> claimFlightSegmentDetails = new();

                Parallel.Invoke(
                            new Action(() => customerResponse = _customerProvider.GetCustomer(null, fcNumber).GetAwaiter().GetResult()),
                            new Action(() => productDetails = _productRefService.Query().GetAwaiter().GetResult()),
                            new Action(() => partnerProfileDetails = _partnerProfileService.Query().GetAwaiter().GetResult()),
                            new Action(() => airportDetails = _airportInfoService.Query().GetAwaiter().GetResult())
                            );

                var partnerDataResponse = await _pdmProvider.PDMGetFlownDetails(ticketNumber);

                if (!Helper.CheckCustomerName(customerResponse.Customer, partnerDataResponse.VSRetroFlownDetails))
                    throw ClaimExceptionResponse(Constants.Application.NameValidationErrorCode, "Name does not match with the FC account", "Name does not match with the FC account");
                if (!Helper.CheckActivityDate(partnerDataResponse.VSRetroFlownDetails, _timeProvider.GetUtcNow().UtcDateTime))
                    throw ClaimExceptionResponse(Constants.Application.DateValidateErrorCode, "Ticket is older than 6 months or less than 48 hours.", "Ticket is older than 6 months or less than 48 hours.");

                var productCode = productDetails.First(x => x.ProductCategory == Application.ProductCategory);
                var partnerCode = partnerProfileDetails.First(x => x.PartnerCode == Application.VSOperatingAirlineCode && x.Name == Application.PartnerName);

                bool IsPartialTransaction = false;
                foreach (var pdmResponse in partnerDataResponse.VSRetroFlownDetails)
                {
                    try
                    {
                        var bookingDtls = (await _httpConnector.SendAsync<IEnumerable<BookingClass>>(ConfigReader.Get(Environment.RefDataMSGenericUrl).Replace("{0}", Application.GetBookingDetailsEndpoint), null, string.Empty, HttpMethod.Post, Json.Serialize(new { FlownDetails = (FlownDetails)pdmResponse }))).Response;

                        //New Process for retrieving Booking details with validation logic in Loyalty MS
                        //Add code when swapping to new process
                        //if (ConfigReader.Try<bool>(Environment.EnableFlagGetBookingPreValidationResponse, false))
                        //{
                        //    var bookingDtlsV2PreValidationResponse = (await _httpConnector.SendAsync<PreValidationPartnerData>(ConfigReader.Get(Environment.RefDataMSGenericUrl).Replace("{0}", Application.GetBookingDetailsEndpointV2), null, string.Empty, HttpMethod.Post, Json.Serialize(new { FlownDetails = (FlownDetails)pdmResponse }))).Response;

                        //    var bookingDtlsV2 = (await _httpConnector.SendAsync<IEnumerable<BookingClass>>(ConfigReader.Get(Environment.LoyaltyMemberManagementMsUrl).Replace("{0}", Application.GetBookingDetailsValidationEndpoint), null, string.Empty, HttpMethod.Post, Json.Serialize(new { BookingClassValidationRequest = (BookingClassValidationRequest)(bookingDtlsV2PreValidationResponse, pdmResponse.BookClass, pdmResponse.FlownCabinClass, pdmResponse.DepartureDate, pdmResponse.FareBasis, pdmResponse.PassengerType, pdmResponse.TicketDesignator) }))).Response;
                        //}

                        var inRuleResponse = (await _inRuleProvider.InRulePointsCalculator(customerResponse.Customer, pdmResponse)).SegmentResponses.First();

                        //Checking for existing transactions for customer 
                        var duplicateTransaction = _customerTransactionDocProvider.CustomerTransactionQuery($"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetTransactionEndPoint}".Replace("{CustomerId}", customerResponse.Customer.CustomerId), customerResponse.Customer.CustomerId).Result.transactionDocuments.Any(n => n.ProductId == productCode.RowId && n.StatusCode == Common.Constants.Application.TxnProcessingStatus && Convert.ToDateTime(n.ActivityDate).Date == Convert.ToDateTime(inRuleResponse.FlightDate).Date && n.FlightDetails.OriginAirportcode == pdmResponse.DepartureAirport && n.FlightDetails.DestinationAirport == pdmResponse.ArrivalAirport && n.TransactionType == Common.Constants.Application.AccrualTransactionType);

                        if (duplicateTransaction)
                            throw ClaimExceptionResponse(Constants.Application.VredCreateTransactionErrorCode, "Duplicate Transaction found", "Duplicate Transaction found");

                        var statementText = await _loyaltyMemberManagementProvider.GenerateStatementText(MissingPointsMapper.GetStatementText(fcNumber, productCode, partnerCode, pdmResponse, "N", bookingDtls.First().CabinClass));

                        string flownBookingClass = !string.IsNullOrEmpty(pdmResponse.FlownUpgradedClass) ? pdmResponse.FlownUpgradedClass : pdmResponse.OriginalBookedClass;

                        var createVredTransaction = MissingPointsMapper.CreateTransactionVRedMapper(customerResponse.Customer, productCode, inRuleResponse, partnerCode, statementText, pdmResponse, flownBookingClass);

                        string segmentDistance = (await _httpConnector.GetAsync<string>(ConfigReader.Get(Environment.RefDataMSGenericUrl).Replace("{0}", Application.GetDistanceEndpoint.Replace("{Origin}", inRuleResponse.Origin).Replace("{Destination}", inRuleResponse.Destination)), null, string.Empty)).Response;

                        var TransactionDetails = await _customerTransactionQueryProvider.CreateTransaction(MissingPointsMapper.MapTransactionReq(customerResponse.Customer, inRuleResponse, pdmResponse, createVredTransaction, partnerCode, productCode, segmentDistance, statementText.StatementText, bookingDtls.First().CabinClass));

                        var createTransaction = await _transactionMutationProvider.CreateVredFlightTransaction(createVredTransaction);

                        if (createTransaction.Status != Common.Entities.Enums.Status.OK)
                            throw ClaimExceptionResponse(Constants.Application.VredCreateTransactionErrorCode, "Error in VRed MS while updating data", "Error in VRed MS while updating data");

                        try
                        {
                            var updateStatus = await _pdmProvider.UpdateTicketStatus(new TicketStatusRequest()
                            {
                                TicketNumber = pdmResponse.TicketNumber,
                                FlownRecordId = Convert.ToString(pdmResponse.Id),
                                TicketStatus = Constants.Application.ProcessedStatus,
                                MemberNumber = fcNumber,
                                LoyaltyProgrammeCode = Application.VSLoyaltyProgrammeCode
                            });

                            if (!string.Equals(updateStatus, AccountApplication.TicketStatusUpdateSuccessMessage, StringComparison.Ordinal))
                            {
                                _logger.LogError("Flown data update failed after a successful VS retro claim. FlownRecordId: {FlownRecordId}, UpdateStatus: {UpdateStatus}",
                                    pdmResponse.Id,
                                    updateStatus);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Flown data update failed after a successful VS retro claim. FlownRecordId: {FlownRecordId}", pdmResponse.Id);
                        }

                        var segmentDetail = new ClaimFlightSegmentDetails
                        {
                            OperatingAirlineName = pdmResponse.OperatingAirlineCode == "VS" ? Constants.Application.VirginAtlantic : null,
                            OperatingAirlineCode = pdmResponse.OperatingAirlineCode,
                            FlightNumber = $"{pdmResponse.OperatingAirlineCode}{pdmResponse.OperatingFlightNumber}",
                            DateOfFlight = pdmResponse.DepartureDate.ToString(Application.ISODateTimeFormat),
                            OriginAirportCode = pdmResponse.DepartureAirport,
                            OriginAirportName = airportDetails.First(x => x.AirportCode == pdmResponse.DepartureAirport).AirportName,
                            OriginAirportCountry = airportDetails.First(x => x.AirportCode == pdmResponse.DepartureAirport).CountryName,
                            DestinationAirportCode = pdmResponse.ArrivalAirport,
                            DestinationAirportName = airportDetails.First(x => x.AirportCode == pdmResponse.ArrivalAirport).AirportName,
                            DestinationAirportCountry = airportDetails.First(x => x.AirportCode == pdmResponse.ArrivalAirport).CountryName,
                            VirginPoints = Convert.ToString(inRuleResponse.PointsAward),
                            BonusPoints = Convert.ToString(inRuleResponse?.TierBonusPoints + inRuleResponse?.CabinBonusPoints),
                            TierPoints = Convert.ToString(inRuleResponse.TierPointsAward),
                        };

                        claimFlightSegmentDetails.Add(segmentDetail);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Partial Failure in VS Claim points {pdmResponse.Id}\r\n{ex.Message}");
                        IsPartialTransaction = true;

                    }
                }

                if (!claimFlightSegmentDetails.Any())
                    throw ClaimExceptionResponse(Constants.Application.VSPointsClaimError, "Error While processing VS Points Claim", "Error While processing VS Points Claim");

                return GetClaimPointResponse(claimFlightSegmentDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled Exception while claiming missing points for VS Flights with Ticket number {ticketNumber}\r\n{ex.Message}");

                if (ex.GetType() == typeof(HttpRequestException))
                {
                    var responseContent = await (ex as HttpRequestException).ReadResponseContentAsStringAsync();
                    var errorDetails = Json.Deserialize<List<ErrorDetails>>(responseContent);
                    if (!errorDetails.IsNullOrEmpty())
                    {
                        var builder = ErrorBuilder.New().SetMessage("VS Points Claim failed");
                        foreach (var error in errorDetails)
                        {
                            builder.SetExtension("code", error.Code);
                            builder.SetExtension("message", error.Message);
                            builder.SetExtension("source", error.ErrorSource);
                        }
                        ;

                        throw new GraphQLException(builder.Build());
                    }
                }
                var errors = ((GraphQLException)ex).Errors.FirstOrDefault();
                string errorCode = errors != null ? errors.Code : Constants.Application.VSPointsClaimError;
                string errorMessage = errors != null ? errors.Message : "Error While processing VS Points Claim";

                throw ClaimExceptionResponse(errors.Code, errors.Message, errors.Message);
            }

        }

        private GraphQLException ClaimExceptionResponse(string code, string message, string exception)
        {
            return new GraphQLException(ErrorBuilder.New()
                            .SetMessage(message)
                            .SetExtension("Exception", exception)
                            .SetCode(code)
                            .Build());
        }

        private MissingPointsResponse<ClaimFlightSegmentDetails> GetClaimPointResponse(IEnumerable<ClaimFlightSegmentDetails> claimFlightSegmentDetails)
        {
            return new MissingPointsResponse<ClaimFlightSegmentDetails>
            {
                TotalBonusPoints = Convert.ToString(GetPoints(claimFlightSegmentDetails.Select(x => x.BonusPoints))),
                TotalTierPoints = Convert.ToString(GetPoints(claimFlightSegmentDetails.Select(x => x.TierPoints))),
                TotalVirginPoints = Convert.ToString(GetPoints(claimFlightSegmentDetails.Select(x => x.VirginPoints))),
                segmentDetails = claimFlightSegmentDetails,
                ErrorResponse = null
            };
        }

        private int GetPoints(IEnumerable<string> points)
        {
            return points
                .Select(s =>
                {
                    bool success = int.TryParse(s, out int num);
                    return success ? num : 0;
                })
                .Sum();
        }

        private MissingPointsResponse<ClaimFlightSegmentDetails> GetMockClaimPointResponse()
        {
            var segmentDetails = new ClaimFlightSegmentDetails
            {
                OperatingAirlineName = "Virgin Atlantic",
                OperatingAirlineCode = "VS",
                FlightNumber = "VS26",
                DateOfFlight = "2024-08-19T12:00:00",
                OriginAirportCode = "LHR",
                OriginAirportName = "London Heathrow",
                OriginAirportCountry = "United Kingdom",
                DestinationAirportCode = "SFO",
                DestinationAirportName = "San Francisco",
                DestinationAirportCountry = "CA",
                VirginPoints = "2320",
                BonusPoints = "1000",
                TierPoints = "50",
            };

            return new MissingPointsResponse<ClaimFlightSegmentDetails>
            {
                TotalBonusPoints = "100",
                TotalTierPoints = "1500",
                TotalVirginPoints = "3000",
                segmentDetails = new[] { segmentDetails },
                ErrorResponse = null
            };
        }
    }
}
