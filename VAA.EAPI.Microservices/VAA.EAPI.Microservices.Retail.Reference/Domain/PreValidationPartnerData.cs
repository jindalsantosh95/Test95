using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{

    public class PreValidationPartnerData
    {
        public Dictionary<string,string> ListOfValues { get; set; }
        public IEnumerable<PartnerTerms> PartnerTerms { get; set; }
        public IEnumerable<PartnerBookingClass> PartnerBookingClasses { get; set; }
        public Distance Distance { get; set; }
    }
}
