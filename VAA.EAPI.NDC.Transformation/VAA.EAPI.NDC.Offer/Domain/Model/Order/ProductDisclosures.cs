using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class ProductDisclosures
    {
        public IEnumerable<Description> Disclosures { get; set; }
        public FareDetail FareRules { get; set; }
        public IEnumerable<BaggageChargeItem> BagRulesAndAllowances { get; set; }


    }
}
