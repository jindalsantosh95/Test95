using System;
using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class Disclosure
    {
        public string Title { get; set; }
        public string Issuer { get; set; }
        public Link Link { get; set; }
        public Link Media { get; set; }
        public IEnumerable<Object> Text { get; set; }


    }
}
