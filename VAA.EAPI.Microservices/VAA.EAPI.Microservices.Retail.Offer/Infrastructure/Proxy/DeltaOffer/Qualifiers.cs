using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Qualifiers
    {
        public IEnumerable<ProgramQualifier> ProgramQualifier { get; set; }
        public IEnumerable<string> SpecialFaresQualifier { get; set; }
        public IEnumerable<DiscountCertificate> DiscountCertificate { get; set; }
        public IEnumerable<AddOnTicketingQualifiers> AddOnTicketingQualifier { get; set; }
        public string TicketingDate { get; set; }
        public IEnumerable<string> WaiverCodes { get; set; }
        public IEnumerable<string> AccountableDocumentNumList { get; set; }
    }
}
