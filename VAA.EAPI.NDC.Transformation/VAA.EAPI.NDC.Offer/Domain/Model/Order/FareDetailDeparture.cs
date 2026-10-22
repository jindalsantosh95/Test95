using System;
using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class FareDetailDeparture
    {
        public string AirportCode { get; set; }
        public DateTime? DepartureDate { get; set; }
        public FareDetailDepartureArrival Arrival { get; set; }
        public string FareBasisCode { get; set; }
        public string AirlineCode { get; set; }
        public string FareCode { get; set; }
        public IEnumerable<FareDetailDepartureRules> Rules { get; set; }
        public IEnumerable<CustomerPrice> FarePrice { get; set; }

    }
}
