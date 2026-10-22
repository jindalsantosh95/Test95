

using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class FarePriceDetail
    {
        public string AdvisoryMessage { get; set; }
        public string BaseAmount { get; set; }
        public string BaseCurrency { get; set; }
        public string BooksCount { get; set; }
        public string DTIType { get; set; }
        public string DtiCount { get; set; }
        public string ExpandedPassengerTypeCode { get; set; }
        public string FarecalcType { get; set; }
        public string InternationalFlag { get; set; }
        public string PassengerTypeCode { get; set; }
        public IEnumerable<object> Tax { get; set; }
        public string TaxItemCount { get; set; }
        public string TotalFareAmount { get; set; }
        public string TotalFareCurrency { get; set; }
        public string TotalFareType { get; set; }
        public string TotalTax { get; set; }
    }
}
