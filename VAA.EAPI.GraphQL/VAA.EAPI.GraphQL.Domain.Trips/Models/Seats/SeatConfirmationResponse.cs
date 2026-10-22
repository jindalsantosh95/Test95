using HotChocolate;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Trips.Constants;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("TripsSeatConfirmationResponse")]
    public class SeatConfirmationResponse
    {
        public string TripReservationId { get; set; }
        public string PnrLocator { get; set; }
        public IEnumerable<FlightPassengerSeatMap> PassengerSeatSelection { get; set; }
        public IEnumerable<Trips.Virgin.Segment> FlightSegments { get; set; }
        public IEnumerable<Common.Entities.Error> Errors { get; set; }
        public IEnumerable<Common.Entities.Warning> Warnings { get; set; }

        public static explicit operator SeatConfirmationResponse((Basket basket, IEnumerable<Infrastructure.Models.BasketMS.FlightPassengerSeatMap> flightPassengerSeatMap, SeatFulfilmentResponse seatFulfilmentResponse, IEnumerable<SeatResponse> seatResponses, IEnumerable<CabinRefData> cabinRefDatas, IEnumerable<AirlineRefData> airlineRefDatas) source)
        {
            var (basket, flightPassengerSeatMap, seatFulfilmentResponse, seatResponses, cabinRefDatas, airlineRefDatas) = source;

            //get segments from flightPassengerSeatMap
            var segmentIds = flightPassengerSeatMap.Select(segment => segment.SectorId).Distinct();
            var slices = basket.BookingDetails.Slices?.Where(slice => slice.Segments.Any(seg => segmentIds.Contains(seg.Id)));

            if (!seatFulfilmentResponse.IsNullOrDefault())
            {
                var seatConfirmationResponse = new SeatConfirmationResponse
                {
                    PnrLocator = basket.BookingDetails.PnrLocator,
                    PassengerSeatSelection = flightPassengerSeatMap.Select(fl =>
                    {
                        var seatsRequested = (FlightPassengerSeatMap)(fl, fl.SeatOfferId, basket.BookingDetails);
                        var seatOfferMap = seatResponses?.FirstOrDefault(sm => sm.SeatOfferId == fl.SeatOfferId);
                        var passenger = basket.BookingDetails.Passenger.FirstOrDefault(p => p.Id == fl.PassengerId);

                        var eligibility = seatOfferMap?.FlightSegmentSeatMap
                            .SelectMany(f => f.Seatmap)
                            .SelectMany(r => r.SeatRows)
                            .SelectMany(c => c.SeatColumns)
                            .FirstOrDefault(s => s.SeatIdentifer == seatsRequested.SeatNumber)?.Eligibility;
                        var selectedOffer = eligibility?.FirstOrDefault(e => e.CustomerId.Contains(passenger.CustomerId) && e.CatalogueRefId == fl.SelectedSeatOfferId);
                        var catalogueData = seatOfferMap.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == selectedOffer?.CatalogueRefId).First();

                        seatsRequested.SelectedFareProductName = cabinRefDatas.FirstOrDefault(c => c.BrandID == catalogueData.BrandId).BrandName;
                        seatsRequested.CustomerId = passenger.CustomerId;
                        if (seatFulfilmentResponse?.OrderItems?.Any(s => s.SeatNumber == fl.SeatNumber && s.FlightIdRef == catalogueData.FlightIdRef) ?? false)
                        {
                            seatsRequested.IsConfirmed = true;
                        }
                        else
                        {
                            seatsRequested.IsConfirmed = false;
                            seatsRequested.SeatNumber = null;
                            seatsRequested.Price = null;
                        }
                        return seatsRequested;
                    }),
                    FlightSegments = segmentIds.Select(seg =>
                    {
                        return (Trips.Virgin.Segment)(slices.SelectMany(s => s.Segments).FirstOrDefault(segment => segment.Id == seg), cabinRefDatas, airlineRefDatas);
                    }),
                    Warnings = seatFulfilmentResponse.Error?.Select(e =>
                    {
                        if(e.IsWarning == "true" && e.Message == "INCOMPLETESEATADDRESPONSE")
                            return new Warning()
                            {
                                Type = SeatResponseWarnings.SeatConfirmationSuccessWithWarnings.ToString(),
                                Message = "Looks like We are not able to present your seats",
                                Details = e.Message
                            };
                        else
                            return new Warning()
                            {
                                Type = SeatResponseWarnings.PartialSeatConfirmation.ToString(),
                                Message = "Some selected seats could not be confirmed",
                                Details = e.Message
                            };
                    })
                };

                return seatConfirmationResponse;
            }
            else
                return new SeatConfirmationResponse()
                {
                    PnrLocator = basket.BookingDetails.PnrLocator,
                    PassengerSeatSelection = flightPassengerSeatMap.Select(fl =>
                    {
                        var seatsRequested = (FlightPassengerSeatMap)(fl, fl.SeatOfferId, basket.BookingDetails);
                        seatsRequested.IsConfirmed = false;
                        seatsRequested.SeatNumber = null;
                        seatsRequested.Price = null;
                        seatsRequested.SelectedFareProductName = null;
                        var passenger = basket.BookingDetails.Passenger.FirstOrDefault(p => p.Id == fl.PassengerId);
                        seatsRequested.CustomerId = passenger?.CustomerId;
                        return seatsRequested;
                    }),
                    FlightSegments = segmentIds.Select(seg =>
                    {
                        return (Trips.Virgin.Segment)(slices.SelectMany(s => s.Segments).FirstOrDefault(segment => segment.Id == seg), cabinRefDatas, airlineRefDatas);
                    }),
                    Errors = seatFulfilmentResponse?.Error?.Select(e => (Common.Entities.Error)e)
                };
        }
    }
}
