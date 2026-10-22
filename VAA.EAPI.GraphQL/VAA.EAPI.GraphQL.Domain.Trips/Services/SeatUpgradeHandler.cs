using HotChocolate;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class SeatUpgradeHandler : ISeatUpgradeHandler
    {
        public Task<Warning> AddErrorForInvalidUpgradeBasket(Basket basket)
        {
            throw new GraphQLException(ErrorBuilder.New()
                  .SetMessage("Seat Upgrade Invalid")
                  .SetExtension("Response", Json.Serialize(new List<ErrorDetails>()
                  {
                        new() {
                            Code = Constants.ErrorCodes.InvalidSeatUpgradeConfirmation,
                            DeveloperMessage = $"Seat Upgrade Validation Failed for {basket.BookingDetails.PnrLocator} | ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                            Message = $"Seat Upgrade Validation Failed"
                        }
                  }))
                  .SetCode(((int)HttpStatusCode.UnprocessableEntity).ToString())
                  .Build());
        }

        public Task<bool> CheckSeatCabinChange(Basket basket, SeatSelectionRequest request, SeatResponse seatMapResponse)
        {
            var isSeatCabinChangeRequested = false;

            var basketProductCode = basket.BookingDetails.Slices?.FirstOrDefault(slice => slice.Segments.Any(seg => seg.Id == request.SegmentId))?.Segments.FirstOrDefault(seg => seg.Id == request.SegmentId).Legs.FirstOrDefault().CabinClass.ProductCode;

            if (!basketProductCode.IsNullOrEmpty() && basketProductCode != "VSLT")
            {
                var requestedSeat = seatMapResponse.FlightSegmentSeatMap.First().Seatmap.SelectMany(seatMap => seatMap.SeatRows).SelectMany(row => row.SeatColumns).Where(column => column.SeatIdentifer == request.SeatNumber);

                var alreadySelectedSeats = basket.FlightPassengerSeatMap.Where(fps => fps.SectorId == request.SegmentId && fps.PassengerId != request.PassengerId).Select(fm => fm.SeatNumber).Where(sn => !sn.IsNullOrEmpty());
                var previousCustomersRequestedSeat = seatMapResponse.FlightSegmentSeatMap.First().Seatmap.SelectMany(seatMap => seatMap.SeatRows).SelectMany(row => row.SeatColumns).Where(column => column.SeatIdentifer != null && alreadySelectedSeats.Contains(column.SeatIdentifer));

                isSeatCabinChangeRequested = previousCustomersRequestedSeat?.Any(rs => !rs.BrandId.Contains(requestedSeat.FirstOrDefault().BrandId)) ?? false;
            }

            return Task.FromResult(isSeatCabinChangeRequested); ;
        }

        public Task<bool> IsSeatUpgradeConfirmationValid(Basket basket, IEnumerable<SeatResponse> seatMSResponses)
        {
            var isSeatUpgradeConfirmationValid = true;

            if (basket.FlightPassengerSeatMap.Any(fpm => !fpm.SelectedSeatOfferId.IsNullOrEmpty()))
            {
                foreach (var segment in basket.FlightPassengerSeatMap.Select(fpm => fpm.SectorId).Distinct())
                {
                    var bookingClass = basket.BookingDetails.Slices?.FirstOrDefault(slice => slice.Segments.Any(seg => seg.Id == segment))?.Segments.FirstOrDefault(seg => seg.Id == segment).Legs.FirstOrDefault().CabinClass.ProductCode;
                    var fpmForSegment = basket.FlightPassengerSeatMap.Where(fpm => fpm.SectorId == segment);
                    var allFpmBrandIds = new HashSet<string>();
                    foreach (var fpm in fpmForSegment)
                    {
                        if (fpm.SelectedSeatOfferId.IsNullOrEmpty())
                        {
                            allFpmBrandIds.Add(bookingClass);
                        }
                        else
                        {
                            var seatOfferMap = seatMSResponses?.FirstOrDefault(sm => sm.SeatOfferId == fpm.SeatOfferId);
                            var seatColumns = seatOfferMap.FlightSegmentSeatMap.First().Seatmap.SelectMany(seatMap => seatMap.SeatRows).SelectMany(row => row.SeatColumns).Where(column => column.SeatIdentifer == fpm.SeatNumber);
                            foreach (var column in seatColumns)
                            {
                                allFpmBrandIds.Add(column.BrandId);
                            }
                        }
                    }
                    if (allFpmBrandIds.Any(b => b != bookingClass))
                    {
                        var firstNotMatching = allFpmBrandIds.First(b => b != bookingClass);
                        isSeatUpgradeConfirmationValid = allFpmBrandIds.All(a => a == firstNotMatching);
                    }
                }
            }
            else
                isSeatUpgradeConfirmationValid = false;
            return Task.FromResult(isSeatUpgradeConfirmationValid);
        }


    }
}
