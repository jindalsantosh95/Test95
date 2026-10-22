namespace VAA.EAPI.Automation.Heimdall.Requests.Helpers.Holidays
{
    public class HolidaysSearchDynamicBuilder
    {
        public static SearchRequest CreateBooking(string departureDate, string gateway, string duration, string bookingType, string location, bool direct, List<(int Adults, List<int> ChildAges, int Infants)> partyData)
        {
            var booking = new SearchRequest
            {
                DepartureDate = departureDate,
                Gateway = gateway,
                Duration = duration,
                BookingType = bookingType,
                Location = location,
                Direct = direct
            };

            foreach (var party in partyData)
            {
                booking.PartyCompositions.Add(new PartyComposition
                {
                    Adults = party.Adults,
                    ChildAges = party.ChildAges ?? new List<int>(),
                    Infants = party.Infants
                });
            }

            return booking;
        }

        public static BookingRequest BuildBookingRequest(string bookingType, string location, string hotelName, string departureDate, string duration, string gateway, List<(int Adults, List<int> ChildAges, int Infants)> partyData, bool direct = true, List<string>? outIATACodes = null, List<string>? inIATACodes = null, List<string>? outCabins = null, List<string>? inCabins = null, List<string>? outFlightNums = null, List<string>? inFlightNums = null)
        {
            var booking = new BookingRequest
            {
                bookingType = bookingType,
                location = location,
                hotelName = hotelName,
                departureDate = departureDate,
                duration = duration,
                gateway = gateway,
                direct = direct,
                outIATACodes = outIATACodes ?? new List<string>(),
                inIATACodes = inIATACodes ?? new List<string>(),
                outCabins = outCabins ?? new List<string>(),
                inCabins = inCabins ?? new List<string>(),
                outFlightNums = outFlightNums ?? new List<string>(),
                inFlightNums = inFlightNums ?? new List<string>()
            };

            // Populate partyCompositions using existing PartyComposition class
            foreach (var party in partyData)
            {
                booking.partyCompositions.Add(new PartyComposition
                {
                    Adults = party.Adults,
                    ChildAges = party.ChildAges ?? new List<int>(),
                    Infants = party.Infants
                });
            }

            return booking;
        }
    }
}