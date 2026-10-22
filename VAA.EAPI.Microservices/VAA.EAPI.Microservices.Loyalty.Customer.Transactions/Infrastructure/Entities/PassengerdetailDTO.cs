using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class PassengerdetailDTO
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
        public List<SegmentdetailDTO> SegmentDetails { get; set; }

        public PassengerdetailDTO()
        {

        }

        public static explicit operator PassengerdetailDTO(Passengerdetail passengerdetail)
        {
            return new()
            {
                PassengerId = string.IsNullOrEmpty(passengerdetail?.PassengerId) ? Utility.GetId() : passengerdetail?.PassengerId,
                SourceIdentifier = passengerdetail?.SourceIdentifier,
                SourceSystem = passengerdetail?.SourceSystem,
                CreatedOn = string.IsNullOrEmpty(passengerdetail?.PassengerId) ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : passengerdetail.CreatedOn?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                CreatedBy = passengerdetail?.CreatedBy,
                LastUpdatedOn = string.IsNullOrEmpty(passengerdetail?.PassengerId) ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : passengerdetail.LastUpdatedOn?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = passengerdetail?.LastUpdatedBy,
                PassengerName = passengerdetail?.PassengerName,
                PassengerNumber = passengerdetail?.PassengerNumber,
                CertificateNumber = passengerdetail?.CertificateNumber,
                ValueRedeemed = !string.IsNullOrEmpty(passengerdetail?.ValueRedeemed) ? ((int)Convert.ToDouble(passengerdetail?.ValueRedeemed)).ToString() : string.Empty,
                Status = passengerdetail?.Status,
                PartnerLocation = passengerdetail?.PartnerLocation,
                VAReferenceAgreementNumber = passengerdetail?.VAReferenceAgreementNumber,
                NumberOfSegments = passengerdetail?.NumberOfSegments,
                PassengerTypeCode = passengerdetail?.PassengerTypeCode,
                PhysicalTicketNumber = passengerdetail?.PhysicalTicketNumber,
                TicketType = passengerdetail?.TicketType,
                ValueToCredit = passengerdetail?.ValueToCredit,
                NumberOfTickets = passengerdetail?.NumberOfTickets,
                TicketDocumentNumber = passengerdetail?.TicketDocumentNumber,
                TotalAmountSpent = passengerdetail?.TotalAmountSpent,
                SegmentDetails = passengerdetail?.SegmentDetails?.Select(x => (SegmentdetailDTO)x).ToList()
            };
        }
    }
}
