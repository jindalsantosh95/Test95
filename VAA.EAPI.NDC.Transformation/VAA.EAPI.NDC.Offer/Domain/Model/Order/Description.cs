using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class Description
    {
        public string TheText { get; set; }
        public string MarkupStyle { get; set; }
        public Link Link { get; set; }
        public IEnumerable<Link> Media { get; set; }


    }
}
