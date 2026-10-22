using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Core;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class AncillaryDetails : BaseProduct
    {
        public ProductSummaryDetails Summary { get; set; }
        public string BookedUser { get; set; }
        public DateTime? BookedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public RateInfo Rate { get; set; }
        public TravellerInfo TravellersComposition { get; set; }

        public static explicit operator AncillaryDetails(JToken source)
        {
            if (source == null)
                return null;

            var selectedProductOption = source["selectedOptions"]?.FirstOrDefault();
            if (selectedProductOption == null) return null;

            var productCode = selectedProductOption["productCode"]?.Value<string>();

            return new()
            {
                BookedDate = selectedProductOption["bookedDate"]?.Value<DateTime>(),
                BookedUser = selectedProductOption["bookedUser"]?.Value<string>(),
                ModifiedDate = selectedProductOption["modifiedDate"]?.Value<DateTime>(),
                TbxProductId = productCode,
                TravellersComposition = (TravellerInfo)source["travellerInfo"],
                Summary = (ProductSummaryDetails)selectedProductOption["summary"]
            };
        }
    }
}
