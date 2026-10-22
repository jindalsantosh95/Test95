using System;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.TransactionStatement
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

        public static explicit operator StatementTextRequest((TransactionCreateEventData transactionData, Domain.FlyingClubMember.FlyingClubMember flyingClubMember, Domain.Entities.PartnersRetrieval.PartnerProfile partner) source)
        {
            ArgumentNullException.ThrowIfNull(source.transactionData);
            ArgumentNullException.ThrowIfNull(source.transactionData.TransactionDocument);
            ArgumentNullException.ThrowIfNull(source.flyingClubMember);
            ArgumentNullException.ThrowIfNull(source.partner);

            var transactionDocument = source.transactionData.TransactionDocument;
            var flightDetails = transactionDocument.FlightDetails;

            return new StatementTextRequest
            {
                TransactionType = transactionDocument.TransactionType,
                TransactionSubType = transactionDocument.TransactionSubType,
                ProductName = source.transactionData.ProductName,
                ProductDescription = source.transactionData.ProductName,
                PartnerName = source.partner.Name,
                PartnerCode = source.partner.PartnerCode,
                ActivityNumber = transactionDocument.ActivityNumber,
                ExternalReservationNumber = flightDetails?.ExternalReservationNumber,
                OriginAirport = flightDetails?.OriginAirportcode,
                DestinationAirport = flightDetails?.DestinationAirport,
                Quantity = flightDetails?.Quantity,
                MemberType = source.flyingClubMember.MemberType,
                MemberMediaCode = source.flyingClubMember.MediaCode,
                MarketingCarrierCode = flightDetails?.MarketingPartnerCode,
                CabinClass = flightDetails?.CabinClass,
                VACodeShareFlag = flightDetails?.CodeShareFlag,
                MergedMemberNumber = source.flyingClubMember.MergeToMemberNumber,
                MemberNumber = source.flyingClubMember.MemberNumber,
                ClassCode = transactionDocument.ClassCode
            };
        }
    }
}
