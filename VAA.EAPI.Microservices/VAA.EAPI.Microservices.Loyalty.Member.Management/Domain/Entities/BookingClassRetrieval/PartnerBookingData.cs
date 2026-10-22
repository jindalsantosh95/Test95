using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BookingClassRetrieval
{
    public class PreValidationPartnerData
    {
        public Dictionary<string, string> ListOfValues { get; set; }
        public IEnumerable<PartnerTerms> PartnerTerms { get; set; }
        public IEnumerable<PartnerBookingClass> PartnerBookingClasses { get; set; }
        public Distance? Distance { get; set; }
    }
}
