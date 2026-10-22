using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    [GraphQLName("TripsUpdateSeatBasketRequest")]
    public class SeatUpdateRequest
    {
        public string BasketId { get; set; }
        public IEnumerable<FlightPassengerSeatMap> FlightPassengerSeatMap { get; set; }

        public static explicit operator SeatUpdateRequest((Basket basket, string segmentId, string passengerId, string seatNumber, string seatOfferId, string selectedSeatOfferId, SeatResponse seatOfferRetrieveResponse) source)
        {
            var (basket, segmentId, passengerId, seatNumber, seatOfferId, selectedSeatOfferId, seatOfferRetrieveResponse) = source;

            var pos = seatOfferRetrieveResponse.FlightSegmentSeatMap.First().Seatmap.SelectMany(row => row.SeatRows).SelectMany(col => col.SeatColumns).FirstOrDefault(col => col.SeatIdentifer != null && col.SeatIdentifer == seatNumber)?.Pos;
            var price = seatOfferRetrieveResponse.FlightSegmentSeatMap.First().Catalogue.FirstOrDefault(x => x.CatalogueId == selectedSeatOfferId)?.SeatPrice;

            return new()
            {
                BasketId = basket.BasketId.ToString(),
                FlightPassengerSeatMap = basket.FlightPassengerSeatMap.Where(f => f.SectorId == segmentId).Select(f =>
                {
                    if (f.PassengerId == passengerId)
                    {
                        return new FlightPassengerSeatMap()
                        {
                            SectorId = f.SectorId,
                            PassengerId = f.PassengerId,
                            SeatNumber = seatNumber,
                            SeatOfferId = seatOfferId,
                            SelectedSeatOfferId = selectedSeatOfferId,
                            Pos = pos ?? string.Empty,
                            Price = new()
                            {
                                Amount = price != null ? Convert.ToDecimal(price.MonetaryAmount) : 0,
                                Currency = price != null ? price.BaseCurrency : string.Empty
                            },
                            LinkedInfantId = f.LinkedInfantId,
                        };
                    }
                    else
                        return f;
                })
            };
        }

        public static explicit operator SeatUpdateRequest((Basket basket, string segmentId, IEnumerable<string> passengerIds, string seatNumber, string seatOfferId, string selectedSeatOfferId, SeatResponse seatOfferRetrieveResponse) source)
        {
            var (basket, segmentId, passengerIds, seatNumber, seatOfferId, selectedSeatOfferId, seatOfferRetrieveResponse) = source;

            var pos = seatOfferRetrieveResponse.FlightSegmentSeatMap.First().Seatmap.SelectMany(row => row.SeatRows).SelectMany(col => col.SeatColumns).FirstOrDefault(col => col.SeatIdentifer != null && col.SeatIdentifer == seatNumber)?.Pos;
            var price = seatOfferRetrieveResponse.FlightSegmentSeatMap.First().Catalogue.FirstOrDefault(x => x.CatalogueId == selectedSeatOfferId)?.SeatPrice;

            return new()
            {
                BasketId = basket.BasketId.ToString(),
                FlightPassengerSeatMap = basket.FlightPassengerSeatMap.Where(f => f.SectorId == segmentId).Select(f =>
                {
                    if (passengerIds.Contains(f.PassengerId))
                    {
                        return new FlightPassengerSeatMap()
                        {
                            SectorId = f.SectorId,
                            PassengerId = f.PassengerId,
                            SeatNumber = seatNumber,
                            SeatOfferId = seatOfferId,
                            SelectedSeatOfferId = selectedSeatOfferId,
                            Pos = pos ?? string.Empty,
                            Price = new()
                            {
                                Amount = price != null ? Convert.ToDecimal(price.MonetaryAmount) : 0,
                                Currency = price != null ? price.BaseCurrency : string.Empty
                            },
                            LinkedInfantId = f.LinkedInfantId,
                        };
                    }
                    else
                        return f;
                })
            };
        }

        public static explicit operator SeatUpdateRequest((Basket basket, string segmentId, string seatOfferId) source)
        {
            var (basket, segmentId, seatOfferId) = source;

            return new()
            {
                BasketId = basket.BasketId.ToString(),
                FlightPassengerSeatMap = basket.FlightPassengerSeatMap.Where(f => f.SectorId == segmentId).Select(f =>
                {
                    
                        return new FlightPassengerSeatMap()
                        {
                            SectorId = f.SectorId,
                            PassengerId = f.PassengerId,
                            SeatNumber = string.Empty,
                            SeatOfferId = seatOfferId,
                            SelectedSeatOfferId = string.Empty,
                            Pos = string.Empty,
                            Price = new()
                            {
                                Amount = 0m,
                                Currency = string.Empty
                            },
                            LinkedInfantId = f.LinkedInfantId,
                        };
                   
                })
            };
        }
    }
}