using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BookingClassRetrieval
{
    public class PartnerBookingClass
    {
        public string RedemptionClassIndicator { get; set; }
        public string BookingClass { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CabinClass { get; set; }
        public string PartnerCode { get; set; }
    }
}
