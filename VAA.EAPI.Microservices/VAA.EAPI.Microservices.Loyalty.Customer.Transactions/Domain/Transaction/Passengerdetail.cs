using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction
{
    public class Passengerdetail
    {
        public string SourceIdentifier { get; set; }
        public string PassengerId { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
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

        public Passengerdetail()
        {

        }
        public Passengerdetail(PassengerdetailDTO passengerdetail)
        {
            PassengerId = passengerdetail?.PassengerId;
            SourceIdentifier = passengerdetail?.SourceIdentifier;
            SourceSystem = passengerdetail?.SourceSystem;
            CreatedOn = !string.IsNullOrEmpty(passengerdetail?.CreatedOn) ? DateTime.Parse(passengerdetail?.CreatedOn) : null;
            CreatedBy = passengerdetail?.CreatedBy;
            LastUpdatedOn = !string.IsNullOrEmpty(passengerdetail?.LastUpdatedOn) ? DateTime.Parse(passengerdetail?.LastUpdatedOn) : null;
            LastUpdatedBy = passengerdetail?.LastUpdatedBy;
            PassengerName = passengerdetail?.PassengerName;
            PassengerNumber = passengerdetail?.PassengerNumber;
            CertificateNumber = passengerdetail?.CertificateNumber;
            ValueRedeemed = passengerdetail?.ValueRedeemed;
            Status = passengerdetail?.Status;
            PartnerLocation = passengerdetail?.PartnerLocation;
            VAReferenceAgreementNumber = passengerdetail?.VAReferenceAgreementNumber;
            NumberOfSegments = passengerdetail?.NumberOfSegments;
            PassengerTypeCode = passengerdetail?.PassengerTypeCode;
            PhysicalTicketNumber = passengerdetail?.PhysicalTicketNumber;
            TicketType = passengerdetail?.TicketType;
            ValueToCredit = passengerdetail?.ValueToCredit;
            NumberOfTickets = passengerdetail?.NumberOfTickets;
            TicketDocumentNumber = passengerdetail?.TicketDocumentNumber;
            TotalAmountSpent = passengerdetail?.TotalAmountSpent;

            SegmentDetails = new List<Segmentdetail>();
            if (passengerdetail.SegmentDetails != null)
            {
                foreach (SegmentdetailDTO segmentDetails in passengerdetail.SegmentDetails)
                {
                    Segmentdetail segmentdetail = new Segmentdetail(segmentDetails);
                    SegmentDetails.Add(segmentdetail);
                }
            }
        }
    }

}
