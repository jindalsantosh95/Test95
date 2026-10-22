using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Seat;
using VAA.EAPI.GraphQL.Domain.Book.Models.Seats;
using VAA.EAPI.GraphQL.Domain.Book.Validation;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]

    public class RetrieveSeatMapQuery
    {
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly IRefServiceProvider _referenceServiceProvider;
        private readonly ISeatServiceProvider _seatServiceProvider;
        private readonly ILOPAServiceProvider _lopaServiceProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ICustomerProvider _customerProvider;
        private readonly ILogger<RetrieveSeatMapQuery> _logger;

        public RetrieveSeatMapQuery(ILogger<RetrieveSeatMapQuery> logger, IBasketServiceProvider basketServiceProvider, IOfferServiceProvider offerServiceProvider, IRefServiceProvider referenceServiceProvider, ISeatServiceProvider seatServiceProvider, ILOPAServiceProvider lopaServiceProvider, ITokenB2CProvider<string> tokenB2CProvider, ICustomerProvider customerProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(offerServiceProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _seatServiceProvider = seatServiceProvider ?? throw new ArgumentNullException(nameof(seatServiceProvider));
            _lopaServiceProvider = lopaServiceProvider ?? throw new ArgumentNullException(nameof(lopaServiceProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        [GraphQLName("retrieveSeatMap")]
        [GraphQLDescription("Retrieves seat map for a flight segment")]
        public async Task<SeatMapResponse> RetrieveSeatMap(SeatMapRequest request)
        {
            try
            {
                if (ConfigReader.Try<bool>(Constants.Environment.SeatMockResponse, false))
                {
                    return Json.Deserialize<SeatMapResponse>(File.ReadAllText($"{Common.Constants.Application.FunctionAppDirPath}/MockResponses/SeatMapResponse.json"));
                }

                var seatMapResponse = new SeatMapResponse();

                if (request == null)
                    throw new ArgumentNullException(nameof(SeatMapRequest));

                if (string.IsNullOrWhiteSpace(request.BasketId))
                    throw new ArgumentException($"'{nameof(request.BasketId)}' must be specified.");

                if (string.IsNullOrWhiteSpace(request.SectorId))
                    throw new ArgumentException($"'{nameof(request.SectorId)}' must be specified.");

                var basket = await _basketServiceProvider.RetrieveBasketAsync(request.BasketId);

                var offerId = basket.OfferIds.First();

                if (string.IsNullOrWhiteSpace(offerId))
                    throw new ArgumentException($"'{nameof(offerId)}'  must be spefied.");

                IEnumerable<AirportRefData> refAirportData = Enumerable.Empty<AirportRefData>();
                IEnumerable<AirlineRefData> refAirlineData = Enumerable.Empty<AirlineRefData>();
                IEnumerable<CabinRefData> refCabinData = Enumerable.Empty<CabinRefData>();
                OfferResponse offer = null;

                Parallel.Invoke(
                        new Action(() => refCabinData = _referenceServiceProvider.RetrieveCabinDataAsync().GetAwaiter().GetResult()),
                        new Action(() => offer = _offerServiceProvider.OfferRetrieveAsync(basket.OfferIds.First()).GetAwaiter().GetResult()),
                        new Action(() => refAirportData = _referenceServiceProvider.RetrieveAirportDataAsync().GetAwaiter().GetResult()),
                        new Action(() => refAirlineData = _referenceServiceProvider.RetrieveAirlineDataAsync().GetAwaiter().GetResult())
                      );

                if (basket is not null && !TokenHelper.GetAuthorizationToken().IsNullOrEmpty())
                {
                    var fQTNumber = TokenHelper.GetFQTNumber(_tokenB2CProvider).Result;
                    var customerFQTNumber = basket?.CustomerProfile?.MembershipDetails.FQTNumber;

                    if (!string.IsNullOrEmpty(customerFQTNumber) && !string.Equals(fQTNumber, customerFQTNumber))
                    {
                        throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
                    }

                    var customer = await _customerProvider.GetCustomer(string.Empty, fQTNumber);
                    if (customer != null)
                    {
                        var departureTime = DateTime.Parse(offer.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime));
                        var passengerAgeonDepartureDate = ValidateBasket.CalculatePassengerAge(Convert.ToDateTime(customer.Customer.DateOfBirth.ToString()), departureTime);

                        if (passengerAgeonDepartureDate < 16)
                            throw new RequestValidationException(Common.Constants.Application.MinorProfileLoginMessage, new Exception(Common.Constants.Application.MinorProfileLoginErrorCode));
                    }
                }
                else if (basket is not null && basket.FlightsearchRequest.isAwardSearch) throw new SecurityTokenException("Missng Token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));

                basket.Summary = BasketSummaryMapper.MapFlightSummary(offer, basket, refAirportData, refAirlineData, null, refCabinData);

                var offerSegment = offer.DataList.FlightSegments.Where(x => x.Legs.First().LegId == request.SectorId);

                if (string.Equals(offerSegment.First().OperatingCarrier, "VS", StringComparison.OrdinalIgnoreCase) || string.Equals(offerSegment.First().OperatingCarrier, "DL", StringComparison.OrdinalIgnoreCase))
                {
                    var seatMapExistForSector = basket.FlightPassengerSeatMap != null ? basket.FlightPassengerSeatMap.Any(map => map.SectorId == request.SectorId) : false;

                    SeatMSResponse retrieveSeatMapResponse = null;

                    if (seatMapExistForSector)
                    {
                        var seatOfferId = basket.FlightPassengerSeatMap.Where(map => map.SectorId == request.SectorId).FirstOrDefault().SeatOfferId;

                        retrieveSeatMapResponse = await _seatServiceProvider.RetrieveSeatOfferDetailsAsync(seatOfferId);
                    }
                    else
                    {
                        SeatMSRequest retrieveSeatMapRequest = SeatMapper.SeatRequestMapper(basket, offer, request);

                        _logger.LogInformation($"SeatMS RQ:{Json.Serialize(retrieveSeatMapRequest)}"); // temp logging

                        retrieveSeatMapResponse = await _seatServiceProvider.RetrieveSeatMapAsync(retrieveSeatMapRequest);

                        if (retrieveSeatMapResponse.FlightSegmentSeatMap.Any())
                        {
                            var updateBasketResponse = await _basketServiceProvider.UpdateSeatDetailsAsync(SeatMapper.MapBasketSeatDetails(basket, retrieveSeatMapResponse.SeatOfferId, request.SectorId));

                            basket.FlightPassengerSeatMap = updateBasketResponse.FlightPassengerSeatMap;
                        }
                    }

                    if (offerSegment.First().OperatingCarrier == "VS")
                    {
                        var aircraftTemplate = await _lopaServiceProvider.RetrieveAircraftDetailsAsync(offer.DataList.FlightSegments.First(segment => segment.Legs.Any(lg => lg.LegId == request.SectorId)).AirCraft, retrieveSeatMapResponse.FlightSegmentSeatMap.First().FlightSegment.EquipmentCode);

                        seatMapResponse = SeatMapper.SeatResponseMapper(basket, offer, retrieveSeatMapResponse, refCabinData, request.SectorId, aircraftTemplate);

                    }
                    else if (offerSegment.First().OperatingCarrier == "DL")
                    {
                        seatMapResponse = SeatMapper.SeatResponseMapper(basket, offer, retrieveSeatMapResponse, refCabinData, request.SectorId);

                        _logger.LogInformation($"Row count: {seatMapResponse.SeatMap.Rows.Count()}"); //temp logging
                    }
                }
                else
                {
                    // Non VS/DL block.. Will be handled later
                }

                return seatMapResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw;
            }
            catch (RequestValidationException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw;
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw;
            }
            catch (ArgumentException ex) when (ex is ArgumentException || ex is ArgumentNullException)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request?.BasketId} - Exception - {ex}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw new GraphQLException(ErrorBuilder.New()
                             .SetMessage("Unhandled Exception in retrieve seat map query.")
                             .SetExtension("Exception", ex.Message)
                             .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                             .Build());
            }
        }
    }
}