using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;

namespace VAA.EAPI.GraphQL.Domain.Trips.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class SelectSeatMutation
    {
        private readonly IBasketProvider _basketServiceProvider;
        private readonly ISeatProvider _seatServiceProvider;
        private readonly ILOPAProvider _lopaServiceProvider;
        private readonly IReferenceServiceProvider _referenceServiceProvider;
        private readonly ILogger<SelectSeatMutation> _logger;
        private readonly ISeatUpgradeHandler _seatUpgradeHandler;

        public SelectSeatMutation(IBasketProvider basketServiceProvider, ISeatProvider seatServiceProvider, ILOPAProvider lopaServiceProvider, IReferenceServiceProvider referenceServiceProvider, ILogger<SelectSeatMutation> logger, ISeatUpgradeHandler seatUpgradeHandler)
        {
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _seatServiceProvider = seatServiceProvider ?? throw new ArgumentNullException(nameof(seatServiceProvider));
            _lopaServiceProvider = lopaServiceProvider ?? throw new ArgumentNullException(nameof(lopaServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _seatUpgradeHandler = seatUpgradeHandler ?? throw new ArgumentNullException(nameof(seatUpgradeHandler));
        }

        [GraphQLName("selectSeatMutation")]
        [GraphQLDescription("Select seat per passenger post sale for requested segment")]
        public async Task<SeatMapResponse> SelectSeat(SeatSelectionRequest selectSeatRequest)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(selectSeatRequest);
                ArgumentNullException.ThrowIfNull(selectSeatRequest.TripReservationId);
                ArgumentNullException.ThrowIfNull(selectSeatRequest.SegmentId);
                ArgumentNullException.ThrowIfNull(selectSeatRequest.PassengerId);
                ArgumentNullException.ThrowIfNull(selectSeatRequest.SeatOfferId);

                var seatMapResponse = new SeatMapResponse();

                //get basketId and retrieve
                var basketId = await BookingIdHelper.GetBasketId(selectSeatRequest.TripReservationId);

                var basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);
                if(!basket.FlightPassengerSeatMap.IsNullOrDefault() && basket.FlightPassengerSeatMap.Any())
                {
                    ArgumentOutOfRangeException.ThrowIfNotEqual(basket.FlightPassengerSeatMap.First(fpsm => fpsm.SectorId == selectSeatRequest.SegmentId).SeatOfferId, selectSeatRequest.SeatOfferId, "Seat Offer Id does not match with the selected seat offer in basket.");                    
                }
                var seatOfferRetrieveResponse = await _seatServiceProvider.RetrieveSeatOfferDetailsAsync(selectSeatRequest.SeatOfferId);
                var cabinDetails = await _referenceServiceProvider.RetrieveCabinDataAsync();
                var isSeatCabinChangeRequested = false;

                if (basket.BookingDetails != null)
                {
                    var selectedSeatOfferId = string.Empty;

                    //check if seat de-selection request
                    if (!selectSeatRequest.SeatNumber.IsNullOrEmpty())
                    {
                        var customerId = basket.BookingDetails.Passenger.First(p => p.Id == selectSeatRequest.PassengerId).CustomerId;
                        selectedSeatOfferId = seatOfferRetrieveResponse.FlightSegmentSeatMap.First().Seatmap.SelectMany(seatMap => seatMap.SeatRows).SelectMany(row => row.SeatColumns).Where(column => column.SeatIdentifer == selectSeatRequest.SeatNumber).SelectMany(column => column.Eligibility).Where(eligibility => eligibility.CustomerId.Contains(customerId)).Select(eligibility => eligibility.CatalogueRefId).FirstOrDefault();
                    }
                    else
                    {
                        selectedSeatOfferId = null;
                    }

                    // update selected seat details in basket
                    var updateBasketResponse = await _basketServiceProvider.UpdateSeatDetailsAsync((SeatUpdateRequest)(basket, selectSeatRequest.SegmentId, selectSeatRequest.PassengerId, selectSeatRequest.SeatNumber, selectSeatRequest.SeatOfferId, selectedSeatOfferId, seatOfferRetrieveResponse));
                    basket.FlightPassengerSeatMap = updateBasketResponse.FlightPassengerSeatMap;

                    //update seat map with selection
                    var retrieveSeatMapWithSelectionResponse = await _seatServiceProvider.RetrieveSeatMapWithSelectionAsync((SeatMapSelectionRequest)(basket, selectSeatRequest.SegmentId, selectSeatRequest.SeatOfferId));

                    var carrierCode = basket.BookingDetails.Slices?.FirstOrDefault(slice => slice.Segments.Any(seg => seg.Id == selectSeatRequest.SegmentId))?.Segments.FirstOrDefault(seg => seg.Id == selectSeatRequest.SegmentId)?.OperatingCarrier.CarrierCode;

                    if (carrierCode == "VS")
                    {
                        try
                        {
                            isSeatCabinChangeRequested = await _seatUpgradeHandler.CheckSeatCabinChange(basket, selectSeatRequest, retrieveSeatMapWithSelectionResponse);

                            if (isSeatCabinChangeRequested)
                            {
                                var passengerIds = basket.FlightPassengerSeatMap.Where(fs => fs.SectorId == selectSeatRequest.SegmentId && fs.PassengerId != selectSeatRequest.PassengerId).Select(s => s.PassengerId);

                                updateBasketResponse = await _basketServiceProvider.UpdateSeatDetailsAsync((SeatUpdateRequest)(basket, selectSeatRequest.SegmentId, passengerIds, null, selectSeatRequest.SeatOfferId, null, seatOfferRetrieveResponse));
                                basket.FlightPassengerSeatMap = updateBasketResponse.FlightPassengerSeatMap;

                                //update seat map with selection
                                retrieveSeatMapWithSelectionResponse = await _seatServiceProvider.RetrieveSeatMapWithSelectionAsync((SeatMapSelectionRequest)(basket, selectSeatRequest.SegmentId, selectSeatRequest.SeatOfferId));

                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Could Not Determine the seat Upgrade for Booking reference - {selectSeatRequest?.TripReservationId} | Error message - {ex.Message}");
                        }
                        
                        var lopaTemplate = await _lopaServiceProvider.RetrieveAircraftDetailsAsync(string.Empty, retrieveSeatMapWithSelectionResponse.FlightSegmentSeatMap.First().FlightSegment.EquipmentCode);

                        seatMapResponse = (SeatMapResponse)(selectSeatRequest.TripReservationId, retrieveSeatMapWithSelectionResponse, basket, lopaTemplate, selectSeatRequest.SegmentId, cabinDetails);
                    }
                    else if (carrierCode == "DL")
                    {
                        seatMapResponse = (SeatMapResponse)(selectSeatRequest.TripReservationId, retrieveSeatMapWithSelectionResponse, basket, selectSeatRequest.SegmentId, cabinDetails);
                    }
                    else
                    {
                        throw new Exception(Constants.ErrorCodes.InvalidCarrierCodeErrorCode);
                    }
                    seatMapResponse.IsSeatCabinChangeRequested = isSeatCabinChangeRequested;

                }

                return seatMapResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Booking reference - {selectSeatRequest?.TripReservationId} | Error message - {ErrorHelpers.AggregateException(ex)}");

                throw new Exception(Constants.ErrorCodes.SelectSeatErrorCode, ex);
            }

        }

    }
}