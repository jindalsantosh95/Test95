using System;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;
using Product = VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus.Product;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual
{
    public class VredCreateTransactionEventData
    {
        public VredCreateTransactionRequest Request { get; set; }

        public static explicit operator VredCreateTransactionEventData((TransactionDocument transactionDocument, PartnerProfile partner, Product product) source)
        {
            ArgumentNullException.ThrowIfNull(source.transactionDocument);
            ArgumentNullException.ThrowIfNull(source.partner);
            ArgumentNullException.ThrowIfNull(source.product);

            var flightDetails = source.transactionDocument.FlightDetails;

            return new VredCreateTransactionEventData
            {
                Request = new VredCreateTransactionRequest
                {
                    CustomerId = source.transactionDocument.CustomerId,
                    TransactionId = source.transactionDocument.TransactionId,
                    MemberNumber = source.transactionDocument.MemberNumber,
                    ActivityDate = DateTime.TryParse(source.transactionDocument.ActivityDate, out var activityDate) ? activityDate : null,
                    PartnerName = source.partner.Name,
                    PartnerCode = source.partner.PartnerCode,
                    PartnerCategory = source.partner.PartnerType,
                    ProductName = source.product.ProductName,
                    Points = int.TryParse(source.transactionDocument.TransactionValue, out var points) ? points : 0,
                    Comments = source.transactionDocument.Comments,
                    CreatedBy = source.transactionDocument.CreatedBy,
                    UpdatedBy = source.transactionDocument.LastUpdatedBy,
                    TransactionSubType = source.transactionDocument.TransactionSubType,
                    TransactionChannel = source.transactionDocument.TransactionChannel,
                    CancelledTransactionId = source.transactionDocument.CancelledTransactionId,
                    CancellationReason = source.transactionDocument.CancellationReasonCode,
                    CancelledBy = source.transactionDocument.CancelledBy,
                    ActivityType = source.transactionDocument.TransactionType,
                    Status = source.transactionDocument.StatusCode,
                    BookingClass = flightDetails?.BookingClass,
                    TransactionNumber = source.transactionDocument.TransactionId,
                    NoOfPassengers = int.TryParse(flightDetails?.Quantity, out var passengers) ? passengers : 0,
                    NoOfSegments = int.TryParse(flightDetails?.NumberOfSegments, out var segments) ? segments : 0,
                    TicketNumber = flightDetails?.Ticketnumber,
                    TicketIssueDatetime = DateTime.TryParse(flightDetails?.BookingDate, out var ticketIssueDate) ? ticketIssueDate : null,
                    MarketingBookingClass = flightDetails?.MarketingBookingClass,
                    MarketingCarrierCode = flightDetails?.MarketingPartnerCode,
                    MarketingFlightNumber = flightDetails?.MarketingFlightNumber,
                    OperatingAirlineAuthNumber = flightDetails?.OperatingAirlineAuthNumber,
                    OperatingFlightNumber = flightDetails?.OperatingFlightNumber,
                    PdmPartnerFileName = source.transactionDocument.PDMPartnerFileId,
                    OriginationAirport = flightDetails?.OriginAirportcode,
                    DestinationAirport = flightDetails?.DestinationAirport,
                    PointType = string.IsNullOrEmpty(source.transactionDocument.TransactionValue) ? string.Empty : Constants.Application.PointsPointType,
                    BookingReference = flightDetails?.ExternalReservationNumber,
                    PartnerSubType = source.partner.PartnerSubtype,
                    StatementText = source.transactionDocument.StatementText,
                    PostingType = flightDetails?.VAPostingTypeCode,
                    BonusMiles = source.transactionDocument.FinancialCreditDetails?.BonusMiles,
                    ActivityNumber = source.transactionDocument.ActivityNumber,
                    ActivityMultiplier = source.transactionDocument.ActivityMultiplier,
                    RetroIndicatorFlag = flightDetails?.RetroFlag,
                    RetroOverrideFlag = flightDetails?.RetroOverrideFlag,
                    RetroOverrideReason = flightDetails?.RetroOverrideReason,
                    TransferMemberNumber = source.transactionDocument.TransferMemberNumber,
                    TravelDistance = flightDetails?.Distance,
                    BasePoints = source.transactionDocument.CreditDetails?.FirstOrDefault(credit => credit.PointSubType.Is(Constants.Application.BaseMilesPointSubType))?.CreditValue,
                    ClassOfServicePoints = source.transactionDocument.CreditDetails?.FirstOrDefault(credit => credit.PointSubType.Is(Constants.Application.CabinBonusPointSubType))?.CreditValue,
                    EliteTierPoints = source.transactionDocument.CreditDetails?.FirstOrDefault(credit => credit.PointSubType.Is(Constants.Application.TierBonusPointSubType))?.CreditValue,
                    PromotionBonusPoints = source.transactionDocument.CreditDetails?.FirstOrDefault(credit => credit.PointSubType.Is(Constants.Application.PromotionBonusPointSubType))?.CreditValue,
                    ClassCode = source.transactionDocument.ClassCode,
                    CabinClass = flightDetails?.CabinClass,
                    FareBasis = flightDetails?.FareBasis
                }
            };
        }
    }

    public class VredCreateTransactionRequest
    {
        public string CustomerId { get; set; }
        public string TransactionId { get; set; }
        public string MemberNumber { get; set; }
        public DateTime? ActivityDate { get; set; }
        public string PartnerName { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerCategory { get; set; }
        public string ProductName { get; set; }
        public int Points { get; set; }
        public string Comments { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string TransactionSubType { get; set; }
        public string TransactionChannel { get; set; }
        public string CancelledTransactionId { get; set; }
        public string CancellationReason { get; set; }
        public string CancelledBy { get; set; }
        public string ActivityType { get; set; }
        public string Status { get; set; }
        public string BookingClass { get; set; }
        public string TransactionNumber { get; set; }
        public int NoOfPassengers { get; set; }
        public int NoOfSegments { get; set; }
        public string TicketNumber { get; set; }
        public DateTime? TicketIssueDatetime { get; set; }
        public string MarketingBookingClass { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingAirlineAuthNumber { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string PdmPartnerFileName { get; set; }
        public string OriginationAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string PointType { get; set; }
        public string BookingReference { get; set; }
        public string PartnerSubType { get; set; }
        public string StatementText { get; set; }
        public string PostingType { get; set; }
        public string BonusMiles { get; set; }
        public string ActivityNumber { get; set; }
        public string ActivityMultiplier { get; set; }
        public string RetroIndicatorFlag { get; set; }
        public string RetroOverrideFlag { get; set; }
        public string RetroOverrideReason { get; set; }
        public string TransferMemberNumber { get; set; }
        public string TravelDistance { get; set; }
        public string BasePoints { get; set; }
        public string ClassOfServicePoints { get; set; }
        public string EliteTierPoints { get; set; }
        public string PromotionBonusPoints { get; set; }
        public string ClassCode { get; set; }
        public string CabinClass { get; set; }
        public string FareBasis { get; set; }
    }
}
