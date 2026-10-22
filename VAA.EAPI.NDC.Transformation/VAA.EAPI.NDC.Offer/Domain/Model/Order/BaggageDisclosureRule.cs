using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class BaggageDisclosureRule
    {
        public string BagRuleCode { get; set; }
        public IEnumerable<Description> Description { get; set; }
        public IEnumerable<string> Remarks { get; set; }
    }
}
