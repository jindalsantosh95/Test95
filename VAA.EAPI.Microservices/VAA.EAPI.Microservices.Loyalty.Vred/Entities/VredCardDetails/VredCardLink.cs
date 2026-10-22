using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredCardDetails
{
    public class VredCardLink
    {     
        public string CardLinkPartnerUserIdDomain { get; set; }
        public string PartnerName { get; set; }
        public string LinkedDateTime { get; set; }
        public string AnniversaryDateTime { get; set; }
        public string CardType { get; set; }
        public string CancelledDateTime { get; set; }
        public string CardLinkPartnerUserId { get; set; }
    }

    
}
