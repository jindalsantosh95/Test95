using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Core;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class HolidayProductDetails : BaseProduct
    {
        public ProductSummaryDetails Summary { get; set; }
        public string BookedUser { get; set; }
        public DateTime? BookedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public RateInfo Rate { get; set; }
        public TravellerInfo TravellersComposition { get; set; }
        public HotelDetails HotelDetails { get; set; }
        public CruiseDetails CruiseDetails { get; set; }
        public CarDetails? CarDetails { get; set; }
        public FlightDetails FlightDetails { get; set; }
        public AncillaryProductDetails GenericProductDetails { get; set; }
        public SupplierDetails? SupplierDetails { get; set; }
        public bool? IsAncillaryProduct { get; set; }
        public bool? IsDerivedFromHotelSupplement { get; set; }

        private static readonly HashSet<string> AncillaryProductCodes = new(StringComparer.OrdinalIgnoreCase) { "GEN", "TRS" };

        public static explicit operator HolidayProductDetails(JToken source)
        {
            if (source == null) return null;

            bool isAncillaryProduct = false;
            var productCode = source["productCode"]?.Value<string>();
            var productKey = source["productKey"]?.Value<string>();
            var selectedOptionProductCode = source["selectedOptions"]?.FirstOrDefault()?["productCode"]?.Value<string>();

            if (productCode == null && selectedOptionProductCode != null)
            {
                source = source["selectedOptions"]?.FirstOrDefault();
                productCode = source["productCode"]?.Value<string>();
                productKey = source["productKey"]?.Value<string>();
            }

            if (productCode != null && AncillaryProductCodes.Contains(productCode))
            {
                isAncillaryProduct = true;
            }

            return new()
            {
                BookedDate = source["bookedDate"]?.Value<DateTime>(),
                BookedUser = source["bookedUser"]?.Value<string>(),
                ModifiedDate = source["modifiedDate"]?.Value<DateTime>(),
                TbxProductId = productKey,
                TbxProductType = productCode,
                HotelDetails = isAncillaryProduct == false ? productCode == "HTL" ? (HotelDetails)source : default : default,
                CarDetails = isAncillaryProduct == false ? productCode == "CAR" ? (CarDetails?)source : default : default,
                CruiseDetails = isAncillaryProduct == false ? productCode == "CRU" ? (CruiseDetails)source : default : default,
                FlightDetails = isAncillaryProduct == false ? productCode == "FLT" ? (FlightDetails)source : default : default,
                Summary = (ProductSummaryDetails)(source["summary"], source["detail"]),
                TravellersComposition = (TravellerInfo)source["travellerInfo"],
                Rate = (RateInfo)source["rate"],
                GenericProductDetails = isAncillaryProduct ? (AncillaryProductDetails)(source["detail"], productCode) : null,
                SupplierDetails = (SupplierDetails?)source["internal"],
                IsAncillaryProduct = isAncillaryProduct
            };
        }
    }

}
