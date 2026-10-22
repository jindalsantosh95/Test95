using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Core;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsProduct")]
    public partial class Product : BaseProduct
    {
        public string? BookingType { get; set; }
        public Reference? Reference { get; set; }
        public IEnumerable<Slice>? Slices { get; set; }
        public IEnumerable<Passenger>? Passengers { get; set; }
        public TotalPrice? TotalPrice { get; set; }
        public string? Status { get; set; }
        public bool CheckInEligible { get; set; }
        public bool AccessibleTravelEligible { get; set; }

        [Obsolete("For backwards compatibility")]
        public string? PropertyId { get; set; }
        [Obsolete("For backwards compatibility")]
        public string? PropertyName { get; set; }
        [Obsolete("For backwards compatibility")]
        public Address? Address { get; set; }
        [Obsolete("For backwards compatibility")]
        public string? CheckIn { get; set; }
        [Obsolete("For backwards compatibility")]
        public string? CheckOut { get; set; }
        [Obsolete("For backwards compatibility")]
        public string? RoomType { get; set; }
        [Obsolete("For backwards compatibility")]
        public string? RatePlan { get; set; }

        public static explicit operator Product((Infrastructure.Models.TripMS.Product product, IEnumerable<CabinRefData> cabinDetails, IEnumerable<AirlineRefData> airlineDetails) source)
        {
            var (product, cabinDetails, airlineDetails) = source;
            var slices = product.Slices?.Where(x => x.IropProtect == null).ToArray();
            var isCancelled = slices?.Any(VAA.EAPI.GraphQL.Domain.Trips.Helper.CheckInEligibilityHelper.IsCancelled) == true;

            return new Product()
            {
                BookingType = product.BookingType,
                Reference = (Reference)product.Reference,
                Slices = slices?.Select(x => (Slice)(x, cabinDetails, airlineDetails, null, product.Reference?.PnrLocator)),
                Passengers = product.Passengers?.Select(x => (Passenger)(x, Enumerable.Empty<CabinRefData>())),
                TotalPrice = (TotalPrice)product.TotalPrice,
                Status = product.Status,
                CheckInEligible = isCancelled ? false : product.CheckInEligible,
                AccessibleTravelEligible = product.Slices.SelectMany(slice => slice.Segments).Any(segment => segment.OperatingCarrier?.CarrierCode == "VS" || segment.OperatingCarrier?.CarrierCode == "DL"),
                SupportedFlight = true
            };
        }
    }
}
