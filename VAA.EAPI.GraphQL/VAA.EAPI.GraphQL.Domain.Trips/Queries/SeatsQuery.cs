using HotChocolate;
using HotChocolate.Types;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Logging;
using System.Web.Http;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;

namespace VAA.EAPI.GraphQL.Domain.Trips.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class SeatsQuery
    {
        private readonly IBasketProvider _basketServiceProvider;
        private readonly ISeatProvider _seatServiceProvider;
        private readonly ILOPAProvider _lopaServiceProvider;
        private readonly IReferenceServiceProvider _referenceServiceProvider;
        private readonly ILogger<UpsellOffersQuery> _logger;

        public SeatsQuery(IBasketProvider basketServiceProvider, ISeatProvider seatServiceProvider, ILOPAProvider lopaServiceProvider, IReferenceServiceProvider referenceServiceProvider, ILogger<UpsellOffersQuery> logger)
        {
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _seatServiceProvider = seatServiceProvider ?? throw new ArgumentNullException(nameof(seatServiceProvider));
            _lopaServiceProvider = lopaServiceProvider ?? throw new ArgumentNullException(nameof(lopaServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("seatMapQuery")]
        [GraphQLDescription("Retrieve seat map post sale for requested segment")]
        public async Task<SeatMapResponse> SeatMap(string tripReservationId, string segmentId)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(tripReservationId);
                ArgumentNullException.ThrowIfNull(segmentId);

                //get basketId and retrieve
                var basketId = await BookingIdHelper.GetBasketId(tripReservationId);

                var basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);

                var cabinDetails = await _referenceServiceProvider.RetrieveCabinDataAsync();
                SeatMapResponse seatMapResponse = null;

                if (basket?.BookingDetails != null)
                {
                    //prepare seat MS request and retrieve seat map

                    var seatMap = await _seatServiceProvider.RetrieveSeatMapAsync((SeatRequest)(basket.BookingDetails, segmentId));

                    // clear basket
                    basket = await _basketServiceProvider.UpdateSeatDetailsAsync((SeatUpdateRequest)(basket, segmentId, seatMap.SeatOfferId));

                    var carrierCode = basket.BookingDetails.Slices?.FirstOrDefault(slice => slice.Segments.Any(seg => seg.Id == segmentId))?.Segments.FirstOrDefault(seg => seg.Id == segmentId)?.OperatingCarrier.CarrierCode;

                    if (carrierCode == "VS")
                    {
                        var lopaTemplate = await _lopaServiceProvider.RetrieveAircraftDetailsAsync(string.Empty, seatMap.FlightSegmentSeatMap.First().FlightSegment.EquipmentCode);

                        seatMapResponse = (SeatMapResponse)(tripReservationId, seatMap, basket, lopaTemplate, segmentId, cabinDetails);
                    }
                    else if (carrierCode == "DL")
                    {
                        seatMapResponse = (SeatMapResponse)(tripReservationId, seatMap, basket, segmentId, cabinDetails);
                    }

                          
                }
                return seatMapResponse ?? throw new Exception("No SeatOffers Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Booking reference - {tripReservationId} | Error message - {ErrorHelpers.AggregateException(ex)}");
                throw new Exception(Constants.ErrorCodes.SelectSeatErrorCode, ex);
            }
      
        }
    }
}