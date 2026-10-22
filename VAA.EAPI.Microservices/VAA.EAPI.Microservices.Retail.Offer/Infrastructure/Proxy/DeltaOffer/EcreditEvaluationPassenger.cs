using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class EcreditEvaluationPassenger
    {
        public string PassengerId { get; set; }
        public IEnumerable<AccountableDocumentInConnection> AssociatedECreditDocuments { get; set; }
    }
}
