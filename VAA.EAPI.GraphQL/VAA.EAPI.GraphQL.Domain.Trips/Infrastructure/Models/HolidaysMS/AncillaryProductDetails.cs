using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.Holidays;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class AncillaryProductDetails
    {
        public string ElementGroup { get; set; }
        public string ElementGroupName { get; set; }
        public IEnumerable<Location> PickupPoints { get; set; }
        public IEnumerable<Location> DropoffPoints { get; set; }
        public Location Destination { get; set; }
        public Location SelectedPickupPoint { get; set; }
        public Location SelectedDropoffPoint { get; set; }
        public OneWayTripDetails OneWayTrip { get; set; }
        public bool? IsElementGroupDetailsOverridden { get; set; }

        public bool Return { get; set; }

    }
}
