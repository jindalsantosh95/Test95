using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Core;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public partial class Product : BaseProduct
    {
        public string BookingType { get; set; }
        public Reference Reference { get; set; }
        public IEnumerable<Slice> Slices { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public TotalPrice TotalPrice { get; set; }
        public string Status { get; set; }
        public bool CheckInEligible { get; set; }
        public bool AccessibleTravelEligible { get; set; }

        public static explicit operator Product(JToken source)
        {
            if (source == null) return default;

            return new()
            {
                TbxProductId = source["productCode"]!.Value<string>(),
                Slices = ((JArray)(((JArray)source["fareInfo"])?[0]?["flightOptions"]))?.Select(o => (Trips.Slice)o)
            };
        }
    }
}