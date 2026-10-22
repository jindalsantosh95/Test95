using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Products.Seat;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability
{
    public class SeatMapResponseSeatSelectionMapper
    {
        public static List<FlightSegmentSeatMap> MapSeatSelect(List<FlightSegmentSeatMap> flightSegmentSeats, IEnumerable<PassengerSeatInfo> passengerSeats)
        {

            return flightSegmentSeats.Select(flightMap =>
            {
                var map = flightMap;
                var seatMap = flightMap.Seatmap.Select(m =>
                {
                    var singleMap = m;
                    var seatRows = m.SeatRows.Select(row =>
                    {
                        SeatRow singleRow = row;

                        var seatCols = row.SeatColumns.Select(col =>
                        {
                            SeatColumn seatColumn = col;
                            if (seatColumn != null && seatColumn.Seat && passengerSeats.Select(seat => seat.SelectedSeatIdentifier).Contains(seatColumn.SeatIdentifer))
                            {
                                seatColumn.Available = false;
                                seatColumn.Reserved = true;
                            }
                            return seatColumn;
                        });
                        singleRow.SeatColumns = seatCols.ToList();
                        return singleRow;

                    });

                    singleMap.SeatRows = seatRows.ToList();
                    return singleMap;


                });
                map.Seatmap = seatMap.ToList();
                return map;

            }).ToList();

        }
    }
}
