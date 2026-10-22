using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class Booking : Holidays.Booking
    {
        public string Id { get; set; }
        public Pos Pos { get; set; }
        public IEnumerable<Product> Product { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public BookingResponseDetails ResponseStatus { get; set; }
        public TripDetails TripSearchRequest { get; set; }
        public bool? IsAccountLinkedPNR { get; set; }
        public IEnumerable<AdvisoryNote>? AdvisoryNotes { get; set; }
        public BookingSummary? BookingSummary { get; set; }

        public static explicit operator Booking(JObject json)
        {
            if (json["data"]?[0]?["summary"] == null) return default;

            JToken packageResponse = json["data"]?[0];
            JToken packageSummary = packageResponse?["summary"];

            JArray activePackages = new JArray(json["data"]?.OfType<JObject>().SelectMany(d => d?["products"]?.OfType<JObject>().Where(p => IsValidProductStatus(p["summary"]?["productStatus"]?.Value<string>())) ?? []) ?? []);

            JObject tmdPackage = activePackages.OfType<JObject>().FirstOrDefault(p => p["productCode"]?.Value<string>() == "TMD");

            JArray nonTMDConfirmedProducts = new JArray(activePackages.OfType<JObject>().Where(p => p["productCode"]?.Value<string>() != "TMD"));

            JArray packageProducts = new JArray(tmdPackage?["products"]?.Where(p => IsValidProductStatus(p["summary"]?["productStatus"]?.Value<string>())) ?? []);

            JArray confirmedAncillaryProducts = new JArray((tmdPackage?["ancillaryProducts"] as JArray)?.OfType<JObject>()
                                                    .Where(a => a["selectedOptions"]?.OfType<JObject>()
                                                    .Any(o => string.Equals(o["summary"]?["productStatus"]?.Value<string>(), "CONFIRMED", StringComparison.OrdinalIgnoreCase)) == true) ?? []);

            JArray combinedProducts = new((packageProducts ?? []).Concat(nonTMDConfirmedProducts ?? []).Concat(confirmedAncillaryProducts ?? []));

            var disneyProducts = MapDisneyProductsFromHotelProducts(combinedProducts.Where(x => x["productCode"]?.Value<string>() == "HTL"));

            return new Booking
            {
                Id = packageSummary?["id"]?.Value<int>().ToString(),
                Status = packageSummary?["bookingStatusName"]?.Value<string>(),
                Payment = (BookingPayment)(packageSummary, packageResponse?["payments"]),
                HolidayProducts = combinedProducts.Select(x => (HolidayProductDetails)x).Concat(disneyProducts),
                Summary = (ProductSummaryDetails)(tmdPackage?["summary"] ?? packageSummary),
                Travellers = packageResponse?["travellers"]?.Select(t => (TravellerParty)t),
                HolidayPackageDetails = (PackageDetails)(packageResponse?["detail"], combinedProducts),
                AdvisoryNotes = packageResponse?["advNotes"]?.Select(note => (AdvisoryNote)note),
                BookingSummary = (BookingSummary)packageSummary
            };
        }

        private static IEnumerable<HolidayProductDetails> MapDisneyProductsFromHotelProducts(IEnumerable<JToken> tbxProducts)
        {
            return tbxProducts.Where(IsDisneyProduct).SelectMany(product => (product["roomSet"]?["supplements"]?.Children() ?? Enumerable.Empty<JToken>()).Select((supplement, index) => new
            {
                product,
                supplement,
                index
            })).Select(x => new HolidayProductDetails
            {
                Summary = (ProductSummaryDetails)(x.product["summary"], (Name: x.supplement["name"]?.Value<string>(), Highlight: x.supplement["name"]?.Value<string>())),
                BookedDate = x.product["bookedDate"]?.Value<DateTime?>(),
                BookedUser = x.product["bookedUser"]?.Value<string>(),
                ModifiedDate = x.product["modifiedDate"]?.Value<DateTime?>(),
                Rate = (RateInfo)x.product["rate"],
                TravellersComposition = (TravellerInfo)x.product["travellerInfo"],
                GenericProductDetails = new AncillaryProductDetails
                {
                    Destination = (Location)x.product["detail"]?["destination"],
                    ElementGroup = Constants.Application.DisneyElementGroupCode,
                    ElementGroupName = default,
                    IsElementGroupDetailsOverridden = false
                },
                TbxProductId = $"GEN-{x.product["productKey"]?.Value<string>()}~SUP~{(x.supplement["no"]?.Value<int>() ?? (x.index + 1))}",
                TbxProductType = "GEN",
                IsAncillaryProduct = true,
                IsDerivedFromHotelSupplement = true
            });
        }

        private static bool IsDisneyProduct(JToken product)
        {
            var productName = product["summary"]?["name"]?.Value<string>();
            var supplierName = product["supplierInfo"]?["name"]?.Value<string>();

            return (productName?.Contains("disney", StringComparison.OrdinalIgnoreCase) ?? false)
                || (supplierName?.Contains("disney", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private static bool IsValidProductStatus(string productStatus)
        {
            return !string.Equals(productStatus, "QUOTE", StringComparison.OrdinalIgnoreCase) && !string.Equals(productStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase);
        }
    }
}