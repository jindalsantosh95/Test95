using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BookingClassRetrieval
{
    public class BookingClassValidationRequest
    {
        public PreValidationPartnerData PartnerBookingData { get; set; }
        public string bookingClass { get; set; }
        public string flownClass { get; set; }
        public DateTime activityDate { get; set; }
        public string listFareValue { get; set; }
        public string listTicketValue { get; set; }
        public string listPaxValue { get; set; }
    }
}
