using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.SeatAvailability
{
    public class Context
    {
        public bool IncludePrice { get; set; }
        public string CurrencyCode { get; set; }
        public bool IncludeRichContent { get; set; }
        public string RecordLocator { get; set; }
        public string DeviceType { get; set; }
        public IEnumerable<Customer> CustomerChars { get; set; }
    }
}
