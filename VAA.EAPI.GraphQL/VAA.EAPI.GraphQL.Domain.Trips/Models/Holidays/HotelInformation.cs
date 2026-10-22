using HotChocolate;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HotelInformation")]
    public class HotelInformation
    {     
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
        public string HotelId { get; set; }
        public string HotelName { get; set; }
        public IEnumerable<Room> Rooms { get; set; }
        public Address HotelAddress { get; set; }
        public IEnumerable<ContactDetails> PropertyContactInformation { get; set; }
        public LocalDateTime CheckIn { get; set; }
        public LocalDateTime CheckOut { get; set; }        
        public Content Content { get; set; }
        public PartyComposition PartyComposition { get; set; }
        public string AccommodationType { get; set; }
        public string CancellationPolicy { get; set; }
        public IEnumerable<Amenity> Amenities { get; set; }
        public Location? Destination { get; set; }
        public int? NumberOfRooms { get; set; }
        public TravelDuration? TravelDuration { get; set; }

        public static explicit operator HotelInformation((Infrastructure.Models.HolidaysMS.HotelDetails hotel, ProductSummaryDetails summary, TravellerInfo travellersComposition) source)
        {
            var (hotel, summary, travellersComposition) = source;
            if(hotel == null || summary == null) return default;

            return new()
            {
                HotelId = hotel.HotelCode, 
                HotelName = hotel.Name,                
                HotelAddress = new Address
                {
                    AddressLine1 = hotel.StreetName,
                    City = hotel.TownCity,
                    PostalCode = hotel.Postcode,
                    Country = hotel.Country,
                },
                CheckIn = summary.TravelTimings.StartTime.HasValue ? LocalDateTime.FromDateTime(summary.TravelTimings.StartTime.Value) : default,
                CheckOut = summary.TravelTimings.EndTime.HasValue ? LocalDateTime.FromDateTime(summary.TravelTimings.EndTime.Value) : default,
                Rooms = hotel.RoomDetails?.Select(r => (Room)r),
                AccommodationType = hotel.AccommodationType,
                PartyComposition = (PartyComposition)travellersComposition,
                Destination = (Location)(hotel.Destination, hotel.HotelAddress),
                NumberOfRooms = hotel.NumberOfRooms,
                TravelDuration = (TravelDuration)summary.TravelDuration
            };
        }

         
    }
}
