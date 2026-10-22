using NodaTime;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Core;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public partial class Product : BaseProduct
    {
        public HotelInformation? HotelDetails { get; set; }
        public CarInformation? CarDetails { get; set; }
        public Extra? ExtraDetails { get; set; }
        public LocalDateTime SortByDate { get; set; }
        [HotChocolate.GraphQLIgnore]
        internal LocalDateTime? SortByDateInternal { get; set; }
        public bool SupportedFlight { get; set; }

        private static LocalDateTime GetSortByDate(Infrastructure.Models.HolidaysMS.HolidayProductDetails source)
        {
            if (source.Summary?.TravelDates?.StartDate.HasValue == true)
                return LocalDateTime.FromDateTime(source.Summary.TravelDates.StartDate.Value);

            return default;
        }

        public static explicit operator Product((Infrastructure.Models.HolidaysMS.HolidayProductDetails product, IEnumerable<AirportRefData> refAirportData) source)
            => (Product)(source.product, source.refAirportData, Enumerable.Empty<AirlineRefData>());

        public static explicit operator Product((Infrastructure.Models.HolidaysMS.HolidayProductDetails product, IEnumerable<AirportRefData> refAirportData, IEnumerable<AirlineRefData> airlineRefData) source)
        {
            var (product, refAirportData, airlineRefData) = source;

            if (!IsVisibleToCustomer(product))
                return default;

            return new Product()
            {
                SupportedFlight = product.FlightDetails?.IsBookingSourceAir4 ?? false,
                Slices = product.FlightDetails?.Sectors?.Select(x => (Slice)(x, refAirportData, airlineRefData, product.FlightDetails?.IsBookingSourceAir4 ?? false)),
                BookingType = product switch
                {
                    { HotelDetails: not null } => "HOTEL",
                    { CarDetails: not null } => "CAR",
                    { FlightDetails: not null } => "AIRLINE",
                    { TbxProductType: "TRS" } => "TRANSFER",
                    _ => "EXTRA"
                },
                Reference = (Reference)product.FlightDetails?.AirlineLocator,
                HotelDetails = (HotelInformation)(product.HotelDetails, product.Summary, product.TravellersComposition),
                CarDetails = (CarInformation)(product.CarDetails, product.Summary, product.SupplierDetails, product.TravellersComposition),
                ExtraDetails = (StringComparer.OrdinalIgnoreCase.Equals(product?.TbxProductType, "GEN") || StringComparer.OrdinalIgnoreCase.Equals(product?.TbxProductType, "TRS"))
                    ? (Extra)(product.GenericProductDetails, product.Summary, product.TbxProductType)
                    : null,
                Passengers = product.TravellersComposition?.Travellers?.Select(x => (Passenger)x).ToList(),
                SortByDate = GetSortByDate(product),
                SortByDateInternal = DateTimeHelper.ToLocal(product?.Summary?.TravelTimings?.StartTime ?? product?.Summary?.TravelDates?.StartDate),
                TbxProductId = product.TbxProductId,
                TbxProductType = product.TbxProductType
            };
        }

        private static bool IsVisibleToCustomer(HolidayProductDetails product) => !Constants.Environment.HiddenElementGroups.Contains(product?.GenericProductDetails?.ElementGroup) && !Constants.Environment.HiddenElementGroups.Contains(product?.Summary?.ElementGroup);
    }
}
