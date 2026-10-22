using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class CustomerInfo
    {
        public ContactNumbers ContactNumbers { get; set; }
        public IEnumerable<PersonName> PersonName { get; set; }
        public IEnumerable<CustLoyalty> CustLoyalty { get; set; }
        public Corporate Corporate { get; set; }
        public IEnumerable<Email> Email { get; set; }
    }
}
