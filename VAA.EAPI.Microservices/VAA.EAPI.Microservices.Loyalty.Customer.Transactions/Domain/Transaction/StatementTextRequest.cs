using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction
{
    public class StatementTextRequest
    {
        public string TransactionType { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string PartnerName { get; set; }
        public string ActivityNumber { get; set; }
        public string PartnerCode { get; set; }
        public string ExternalReservationNumber { get; set; }
        public string Quantity { get; set; }
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string TransactionSubType { get; set; }
        public string MemberType { get; set; }
        public string MemberMediaCode { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string CabinClass { get; set; }
        public string VACodeShareFlag { get; set; }
        public string MergedMemberNumber { get; set; }
        public string MemberNumber { get; set; }
        public string StatementText { get; set; }
        public string ClassCode { get; set; }
        public string VoucherName { get; set; }

        public static explicit operator StatementTextRequest((TransactionDocument txnDoc, Partner partner, Product product, string voucherName) stmtRequest)
        {
            if (stmtRequest.txnDoc == null) throw new ArgumentNullException(nameof(stmtRequest.txnDoc));

            return new StatementTextRequest
            {
                TransactionType = stmtRequest.txnDoc.TransactionType,
                ProductName = stmtRequest.product?.ProductName,
                ProductDescription = stmtRequest.product?.ProductDescription,
                PartnerName = stmtRequest.partner?.PartnerName,
                PartnerCode = stmtRequest.partner?.PartnerCode,
                ActivityNumber = stmtRequest.txnDoc.ActivityNumber,
                ExternalReservationNumber = stmtRequest.txnDoc.FlightDetails?.ExternalReservationNumber,
                Quantity = stmtRequest.txnDoc.FlightDetails?.Quantity,
                OriginAirport = stmtRequest.txnDoc.FlightDetails?.OriginAirportcode,
                DestinationAirport = stmtRequest.txnDoc.FlightDetails?.DestinationAirport,
                TransactionSubType = stmtRequest.txnDoc.TransactionSubType,
                MarketingCarrierCode = stmtRequest.txnDoc.FlightDetails?.MarketingPartnerCode,
                MemberNumber = stmtRequest.txnDoc.MemberNumber,
                CabinClass = stmtRequest.txnDoc.FlightDetails?.TravelClass,
                StatementText = stmtRequest.txnDoc.StatementText,
                ClassCode = stmtRequest.txnDoc.ClassCode,
                VoucherName = stmtRequest.voucherName
            };
        }
    }
}
