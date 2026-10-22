using HotChocolate;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("TripsSeatMapResponse")]
    public class SeatMapResponse
    {
        public string TripReservationId { get; set; }
        public string PnrLocator { get; set; }
        public Segment FlightSegment { get; set; }
        public IEnumerable<SeatMap> SeatMap { get; set; }
        public IEnumerable<FlightPassengerSeatMap> FlightPassengerSeatMap { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
        public bool? IsSeatCabinChangeRequested { get; set; } = false;
       
        public static explicit operator SeatMapResponse((string bookingId, Infrastructure.Models.SeatMS.SeatResponse seatMap, Infrastructure.Models.BasketMS.Basket basket, IEnumerable<SeatMap> lopaTemplate, string segmentId, IEnumerable<CabinRefData> cabinRefs) source)
        {
            var (bookingId, seatMSResponse, basket, lopa, segmentId, cabinRefs) = source;

            var requestedSegment = basket.BookingDetails.Slices?.FirstOrDefault(slice => slice.Segments.Any(seg => seg.Id == segmentId))?.Segments.FirstOrDefault(seg => seg.Id == segmentId);
            var disableExitFlag = basket.BookingDetails.Passenger.Any(x => x.Type == "CNN" || x.Type == "CHD" || x.Type == "INF");

            return new()
            {
                TripReservationId = bookingId,
                PnrLocator = basket.BookingDetails.PnrLocator,
                FlightSegment = (Segment)(requestedSegment, cabinRefs, null),
                FlightPassengerSeatMap = basket.FlightPassengerSeatMap.Where(f => f.SectorId == segmentId).Select(f =>
                {
                    var seatDetail = (FlightPassengerSeatMap)(f, seatMSResponse.SeatOfferId, basket.BookingDetails);                   
                    if (!f.SelectedSeatOfferId.IsNullOrEmpty())
                    {
                        var seatColumns = seatMSResponse.FlightSegmentSeatMap.First().Seatmap.SelectMany(seatMap => seatMap.SeatRows).SelectMany(row => row.SeatColumns).Where(column => column.SeatIdentifer == f.SeatNumber);
                        var selectedOffer = seatColumns?.SelectMany(column => column.Eligibility).Where(eligibility => eligibility.CatalogueRefId == f.SelectedSeatOfferId)?.FirstOrDefault();
                        if (!selectedOffer.IsNullOrDefault())
                        {
                            var catalogueData = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == selectedOffer?.CatalogueRefId).First();
                            seatDetail.SelectedFareProductName = cabinRefs.FirstOrDefault(c => c.BrandID == catalogueData.BrandId).BrandName;
                        }
                        else
                        {                           
                            seatDetail.SelectedFareProductName = string.Empty;
                        }
                    }
                    
                    return seatDetail;
                }),
                SeatMap = lopa.Select(l => (SeatMap)(l, seatMSResponse, disableExitFlag, cabinRefs))
            };
        }

        public static explicit operator SeatMapResponse((string bookingId, Infrastructure.Models.SeatMS.SeatResponse seatMap, Infrastructure.Models.BasketMS.Basket basket, string segmentId, IEnumerable<CabinRefData> cabinRefs) source)
        {
            var (bookingId, seatMSResponse, basket, segmentId, cabinRefs) = source;

            var requestedSegment = basket.BookingDetails.Slices?.FirstOrDefault(slice => slice.Segments.Any(seg => seg.Id == segmentId))?.Segments.FirstOrDefault(seg => seg.Id == segmentId);
            var disableExitFlag = basket.BookingDetails.Passenger.Any(x => x.Type == "CNN" || x.Type == "CHD" || x.Type == "INF");
            var classType = cabinRefs.FirstOrDefault(c => c.BrandID == basket.BookingDetails.Slices?.FirstOrDefault(slice => slice.Segments.Any(seg => seg.Id == segmentId))?.Segments.FirstOrDefault(seg => seg.Id == segmentId).Legs.FirstOrDefault().CabinClass.ProductCode)?.BrandName;

            return new()
            {
                TripReservationId = bookingId,
                PnrLocator = basket.BookingDetails.PnrLocator,
                FlightSegment = (Segment)(requestedSegment, cabinRefs, null),
                FlightPassengerSeatMap = basket.FlightPassengerSeatMap.Where(f => f.SectorId == segmentId).Select(f =>
                {
                    var seatDetail = (FlightPassengerSeatMap)(f, seatMSResponse.SeatOfferId, basket.BookingDetails);
                    if (!f.SelectedSeatOfferId.IsNullOrEmpty())
                    {
                        var seatColumns = seatMSResponse.FlightSegmentSeatMap.First().Seatmap.SelectMany(seatMap => seatMap.SeatRows).SelectMany(row => row.SeatColumns).Where(column => column.SeatIdentifer == f.SeatNumber);
                        var selectedOffer = seatColumns?.SelectMany(column => column.Eligibility).Where(eligibility => eligibility.CatalogueRefId == f.SelectedSeatOfferId)?.FirstOrDefault();
                        if (!selectedOffer.IsNullOrDefault())
                        {
                            var catalogueData = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == selectedOffer?.CatalogueRefId).First();
                            seatDetail.SelectedFareProductName = cabinRefs.FirstOrDefault(c => c.BrandID == catalogueData.BrandId).BrandName;
                        }
                        else
                        {
                            seatDetail.SelectedFareProductName = string.Empty;
                        }
                    }

                    return seatDetail;
                }),
                SeatMap = seatMSResponse.FlightSegmentSeatMap.First().Seatmap.Select(s => (SeatMap)(seatMSResponse, disableExitFlag, classType, cabinRefs))
            };
        }

        
    }
}