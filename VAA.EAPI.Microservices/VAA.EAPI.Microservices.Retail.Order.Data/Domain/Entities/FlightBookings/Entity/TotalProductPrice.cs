using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class TotalProductPrice
    {
        public string PriceComponentType { get; set; }
        public string PriceComponentName { get; set; }
        public string PriceComponentDescription { get; set; }
        public Amount Amount { get; set; }
    }
}
