using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class Reference
    {
        public string AirlineDesignator { get; set; }
        public string PnrLocator { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ConfirmationNumber { get; set; }
        public string ChainCode { get; set; }
        public DateTime? BookingDate { get; set; }
        public string OrderId { get; set; }

        public static explicit operator Reference(JToken source)
        {
            if (source == null) return null;

            var flightOptions = source["flightOptions"] as JArray;

            if (flightOptions == null) return null;

            var segmentsToken = flightOptions.SelectMany(fo => (fo["segments"] as JArray) ?? new JArray()).ToList();

            return new()
            {
                // The PnrLocator is being populated from the airlineLocator field in the source JSON, get it from jtoken
                PnrLocator = segmentsToken.FirstOrDefault()?["airlineLocator"]?.Value<string>()
            };
        }
    }
}
