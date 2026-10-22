using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OrderCancellation
    {
        public string ReasonForCancel { get; set; }
        public string Status { get; set; }
        public string AgentIdentifier { get; set; }
        public IEnumerable<string> Remarks { get; set; }
        public PriceDetail AmountRefunded { get; set; }

    }
}
