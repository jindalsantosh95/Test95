using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction
{
    public class Passengerdetail
    {
        public string SourceIdentifier { get; set; }
        public string PassengerId { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string PassengerName { get; set; }
        public string PassengerNumber { get; set; }
        public string CertificateNumber { get; set; }
        public string ValueRedeemed { get; set; }
        public string Status { get; set; }
        public string PartnerLocation { get; set; }
        public string VAReferenceAgreementNumber { get; set; }
        public string NumberOfSegments { get; set; }
        public string PassengerTypeCode { get; set; }
        public string PhysicalTicketNumber { get; set; }
        public string TicketType { get; set; }
        public string ValueToCredit { get; set; }
        public string NumberOfTickets { get; set; }
        public string TicketDocumentNumber { get; set; }
        public string TotalAmountSpent { get; set; }
        public List<Segmentdetail> SegmentDetails { get; set; }
    }
}
