using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class FareTypeRequest
    {
        public string FareTypeCode { get; set; }
        public string FareSubTypeCode { get; set; }
    }
}
