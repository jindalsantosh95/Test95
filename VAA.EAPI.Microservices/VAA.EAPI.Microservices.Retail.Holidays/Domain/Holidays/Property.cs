using NodaTime;
using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips;


namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays
{
    public class Property
    {
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
        public string PropertyId { get; set; }
        public string PropertyName { get; set; }
        public IEnumerable<Room> Rooms { get; set; }
        public Address PropertyAddress { get; set; }
        public IEnumerable<ContactDetails> PropertyContactInformation { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public Content Content { get; set; }
        public PartyComposition PartyComposition { get; set; }
        public string AccommodationType { get; set; }
        public string CancellationPolicy { get; set; }
        public IEnumerable<Amenity> Amenities { get; set; }
    }
}
