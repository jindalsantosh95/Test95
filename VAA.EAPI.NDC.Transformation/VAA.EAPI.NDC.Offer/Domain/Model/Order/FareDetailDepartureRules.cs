using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class FareDetailDepartureRules
    {
        public string FareRuleCategory { get; set; }
        public IEnumerable<string> Text { get; set; }

    }
}
