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

namespace VAA.EAPI.GraphQL.Domain.Book.Mutations.Basket
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class UpdateSeatMutation
    {
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly IRefServiceProvider _referenceServiceProvider;
        private readonly ISeatServiceProvider _seatServiceProvider;
        private readonly ILOPAServiceProvider _lopaServiceProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<UpdateSeatMutation> _logger;
        private readonly ICustomerProvider _customerProvider;

        public UpdateSeatMutation(ILogger<UpdateSeatMutation> logger, IBasketServiceProvider basketServiceProvider,
            IOfferServiceProvider offerServiceProvider, IRefServiceProvider referenceServiceProvider,
            ISeatServiceProvider seatServiceProvider, ITokenB2CProvider<string> tokenB2CProvider, ILOPAServiceProvider lopaServiceProvider, ICustomerProvider customerProvider)
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

        [GraphQLName("selectSeat")]
        [GraphQLDescription("Select seat for a passenger from given seat map response")]
        public async Task<SeatMapResponse> UpdateSeat(SelectSeatRequest request)
        {
            try
            {
                if (ConfigReader.Try<bool>(Constants.Environment.SeatMockResponse, false))
                {
                    return Json.Deserialize<SeatMapResponse>(File.ReadAllText($"{Common.Constants.Application.FunctionAppDirPath}/MockResponses/SeatMapResponse.json"));
                }

                var seatMapResponse = new SeatMapResponse();

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (string.IsNullOrWhiteSpace(request.BasketId))
                    throw new ArgumentException($"'{nameof(request.BasketId)}' must be specified.");

                var basket = await _basketServiceProvider.RetrieveBasketAsync(request.BasketId);

                if (basket is not null && basket.FlightsearchRequest.isAwardSearch && TokenHelper.GetAuthorizationToken() == string.Empty) throw new SecurityTokenException("Missng Token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));

                var offerId = basket.OfferIds.First();

                if (string.IsNullOrWhiteSpace(offerId))
                    throw new ArgumentException($"'{nameof(offerId)}'  must be spefied.");

                if (string.IsNullOrWhiteSpace(request.FlightPassengerSeatMap.SeatOfferId))
                    throw new ArgumentException($"'{nameof(request.FlightPassengerSeatMap.SeatOfferId)}'  must be spefied.");

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

                if (TokenHelper.GetAuthorizationToken() != string.Empty)
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
                else if (basket?.CustomerProfile?.MembershipDetails.FQTNumber.IsNullOrEmpty() == false)
                    throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));

                basket.Summary = BasketSummaryMapper.MapFlightSummary(offer, basket, refAirportData, refAirlineData, null, refCabinData);

                var offerSegment = offer.DataList.FlightSegments.Where(x => x.Legs.First().LegId == request.FlightPassengerSeatMap.SectorId);

                var seatOfferRetrieveResponse = await _seatServiceProvider.RetrieveSeatOfferDetailsAsync(request.FlightPassengerSeatMap.SeatOfferId);

                if (request.FlightPassengerSeatMap.SeatNumber.IsNullOrEmpty() == false)
                {
                    var selectedSeatOfferId = seatOfferRetrieveResponse.FlightSegmentSeatMap.First().Seatmap.SelectMany(seatMap => seatMap.SeatRows).SelectMany(row => row.SeatColumns).Where(column => column.SeatIdentifer == request.FlightPassengerSeatMap.SeatNumber).SelectMany(column => column.Eligibility).Where(eligibility => eligibility.CustomerId.Contains(request.FlightPassengerSeatMap.PassengerId)).Select(eligibility => eligibility.CatalogueRefId).FirstOrDefault();
                    request.FlightPassengerSeatMap.SelectedSeatOfferId = selectedSeatOfferId;
                    request.FlightPassengerSeatMap.Price = new()
                    {
                        Amount = Convert.ToDecimal(seatOfferRetrieveResponse.FlightSegmentSeatMap.First().Catalogue.First(x => x.CatalogueId == selectedSeatOfferId).SeatPrice.MonetaryAmount),
                        Currency = seatOfferRetrieveResponse.FlightSegmentSeatMap.First().Catalogue.First(x => x.CatalogueId == selectedSeatOfferId).SeatPrice.BaseCurrency
                    };

                }
                else
                {
                    request.FlightPassengerSeatMap.SelectedSeatOfferId = null;
                    request.FlightPassengerSeatMap.Price = new()
                    {
                        Amount = 0,
                        Currency = ""
                    };
                }

                var updateBasketResponse = await _basketServiceProvider.UpdateSeatDetailsAsync(SeatMapper.MapBasketSeatSelectDetails(basket, request.FlightPassengerSeatMap));

                basket.FlightPassengerSeatMap = updateBasketResponse.FlightPassengerSeatMap;

                // seat MS validations integrations
                IEnumerable<PassengerSeatInfo> passengerDetails = basket.People.Select(passenger => new PassengerSeatInfo()
                {
                    PassengerId = passenger.Id,
                    SelectedSeatIdentifier = passenger.Type != Constants.Application.InfantPtc ? updateBasketResponse.FlightPassengerSeatMap.Where(x => x.PassengerId == passenger.Id).First(s => s.SectorId == request.FlightPassengerSeatMap.SectorId).SeatNumber : "",
                    Ptc = passenger.Type.ToString(),
                });

                var seatMapUpdate = await _seatServiceProvider.RetrieveSeatMapWithSelectionAsync(new SeatMapSelectionMSRequest()
                {
                    SeatOfferId = request.FlightPassengerSeatMap.SeatOfferId,
                    PassengerSeats = passengerDetails
                });

                if (offerSegment.First().OperatingCarrier == "VS")
                {
                    var aircraftTemplate = await _lopaServiceProvider.RetrieveAircraftDetailsAsync(offer.DataList.FlightSegments.First(segment => segment.Legs.Any(lg => lg.LegId == request.FlightPassengerSeatMap.SectorId)).AirCraft, seatMapUpdate.FlightSegmentSeatMap.First().FlightSegment.EquipmentCode);

                    seatMapResponse = SeatMapper.SeatResponseMapper(basket, offer, seatMapUpdate, refCabinData, request.FlightPassengerSeatMap.SectorId, aircraftTemplate);
                }
                else if (offerSegment.First().OperatingCarrier == "DL")
                {
                    seatMapResponse = SeatMapper.SeatResponseMapper(basket, offer, seatMapUpdate, refCabinData, request.FlightPassengerSeatMap.SectorId);

                    _logger.LogInformation($"Row count: {seatMapResponse.SeatMap.Rows.Count()}"); //temp logging
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
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {request.BasketId} - Exception - {ex}");
                throw new GraphQLException(ErrorBuilder.New()
                             .SetMessage("Unhandled Exception in select seat mutation.")
                             .SetExtension("Exception", ex.Message)
                             .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                             .Build());
            }
        }
    }
}
