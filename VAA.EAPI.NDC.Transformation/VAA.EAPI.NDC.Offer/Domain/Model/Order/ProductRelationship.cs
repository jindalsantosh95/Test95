using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class ProductRelationship
    {
        public string ProductCode { get; set; }
        public IEnumerable<string> RelatedProducts { get; set; }


    }
}
