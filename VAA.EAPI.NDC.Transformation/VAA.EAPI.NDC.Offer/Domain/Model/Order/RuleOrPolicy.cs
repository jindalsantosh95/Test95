using System;
using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class RuleOrPolicy
    {
        public string Title { get; set; }
        public DateTime? PolicyStartDate { get; set; }
        public DateTime? PolicyEndDate { get; set; }
        public string Issuer { get; set; }
        public Link Link { get; set; }
        public IEnumerable<Object> Text { get; set; }


    }
}
