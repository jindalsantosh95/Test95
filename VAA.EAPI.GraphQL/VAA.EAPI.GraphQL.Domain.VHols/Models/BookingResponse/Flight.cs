using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    [GraphQLName("Flight_VHols")]
    public class Flight : IHolidayComponent, IBookingStatusAwareComponent
    {
        public List<Sector_VHMMB> Sectors { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        [Obsolete("Use Airline Parties")]
        public string? AirlineLocator { get; set; }
        [Obsolete("Use Airline Parties")]
        public PartyComposition? PartyComposition { get; set; }
        public string? LegType { get; set; }
        public string? BookingStatus { get; set; }
        public string? HashCode { get; set; }
        public Destination? Destination { get; set; }
        public List<AirlineParty> AirlineParties { get; set; }
        public string SortByDate { get; set; }


    }

}

