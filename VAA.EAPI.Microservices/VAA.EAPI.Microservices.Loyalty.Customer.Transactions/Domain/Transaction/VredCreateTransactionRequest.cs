using System;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction
{
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
        public string ProductType { get; set; }
        public string ProductCategory { get; set; }
        public string ProductDescription { get; set; }
        public string TransactionSource { get; set; }
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
        public string VoucherNumber { get; set; }
        public string VoucherType { get; set; }
        public string VoucherStatus { get; set; }
        public string VoucherDescription { get; set; }
        public DateTime? VoucherIssueDate { get; set; }
        public string PdmPartnerFileName { get; set; }
        public DateTime? VoucherExpiryDate { get; set; }
        public string OriginationAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string PointType { get; set; }
        public string PromotionType { get; set; }
        public string BookingReference { get; set; }
        public string LifeTimeMiles { get; set; }
        public string PartnerSubType { get; set; }
        public string PromotionBonusIndicator { get; set; }
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


        public static explicit operator VredCreateTransactionRequest((TransactionDocument transactionData, Partner partnerData, Product productData, Product awardProductdata, Award awardData, string voucherStatus) vredTxnRequest)
        {
            if (vredTxnRequest.transactionData == null) throw new ArgumentNullException(nameof(vredTxnRequest.transactionData));
            if (vredTxnRequest.partnerData == null) throw new ArgumentNullException(nameof(vredTxnRequest.partnerData));
            if (vredTxnRequest.productData == null) throw new ArgumentNullException(nameof(vredTxnRequest.productData));

            return new VredCreateTransactionRequest()
            {
                CustomerId = vredTxnRequest.transactionData.CustomerId,
                TransactionId = vredTxnRequest.transactionData.TransactionId,
                PartnerCode = vredTxnRequest.partnerData.PartnerCode,
                PartnerName = vredTxnRequest.partnerData.PartnerName,
                MemberNumber = vredTxnRequest.transactionData.MemberNumber,
                ActivityType = vredTxnRequest.transactionData.TransactionType,
                Points = int.TryParse(vredTxnRequest.transactionData.TransactionValue, out var pointsValue) ? pointsValue : 0,
                ActivityDate = DateTime.TryParse(vredTxnRequest.transactionData.ActivityDate, out var activityDateValue) ? activityDateValue : null,
                StatementText = vredTxnRequest.transactionData.StatementText,
                CreatedBy = vredTxnRequest.transactionData.CreatedBy,
                UpdatedBy = vredTxnRequest.transactionData.LastUpdatedBy,
                TransactionChannel = vredTxnRequest.transactionData.TransactionChannel,
                Status = vredTxnRequest.transactionData.StatusCode,
                BookingClass = vredTxnRequest.transactionData.FlightDetails?.BookingClass,
                TransactionNumber = vredTxnRequest.transactionData.TransactionId,
                BonusMiles = vredTxnRequest.transactionData.FinancialCreditDetails?.BonusMiles,
                ActivityNumber = vredTxnRequest.transactionData.ActivityNumber,
                OperatingFlightNumber = vredTxnRequest.transactionData.FlightDetails?.OperatingFlightNumber,
                NoOfPassengers = int.TryParse(vredTxnRequest.transactionData.FlightDetails?.Quantity, out var passengersValue) ? passengersValue : 0,
                NoOfSegments = int.TryParse(vredTxnRequest.transactionData.FlightDetails?.NumberOfSegments, out var segmentsValue) ? segmentsValue : 0,
                ProductName = vredTxnRequest.productData.ProductName,
                TicketNumber = vredTxnRequest.transactionData.FlightDetails?.Ticketnumber,
                TicketIssueDatetime = DateTime.TryParse(vredTxnRequest.transactionData.FlightDetails?.BookingDate, out var ticketIssueDateValue) ? ticketIssueDateValue : null,
                OperatingAirlineAuthNumber = vredTxnRequest.transactionData.FlightDetails?.OperatingAirlineAuthNumber,
                TransactionSubType = vredTxnRequest.transactionData.TransactionSubType,
                CancellationReason = vredTxnRequest.transactionData.CancellationReasonCode,
                Comments = vredTxnRequest.transactionData.Comments,
                CancelledTransactionId = vredTxnRequest.transactionData.CancelledTransactionId,
                VoucherNumber = vredTxnRequest.awardData?.AwardNumber,
                VoucherType = vredTxnRequest.awardProductdata?.ProductName,
                VoucherStatus = !string.IsNullOrEmpty(vredTxnRequest.voucherStatus) ? vredTxnRequest.voucherStatus : vredTxnRequest.awardData?.Status,
                VoucherDescription = vredTxnRequest.awardProductdata?.ProductDescription,
                VoucherIssueDate = vredTxnRequest.awardData?.StartDate,
                PartnerCategory = vredTxnRequest.partnerData.PartnerType,
                ProductCategory = vredTxnRequest.productData.ProductCategory,
                ActivityMultiplier = vredTxnRequest.transactionData.ActivityMultiplier,
                PdmPartnerFileName = vredTxnRequest.transactionData.PDMPartnerFileId,
                CancelledBy = vredTxnRequest.transactionData.CancelledBy,
                ProductDescription = vredTxnRequest.productData.ProductDescription,
                RetroIndicatorFlag = vredTxnRequest.transactionData.FlightDetails?.RetroFlag,
                RetroOverrideFlag = vredTxnRequest.transactionData.FlightDetails?.RetroOverrideFlag,
                RetroOverrideReason = vredTxnRequest.transactionData.FlightDetails?.RetroOverrideReason,
                TransferMemberNumber = vredTxnRequest.transactionData.TransferMemberNumber,
                VoucherExpiryDate = vredTxnRequest.awardData?.ExpiryDate,
                MarketingBookingClass = vredTxnRequest.transactionData.FlightDetails?.MarketingBookingClass,
                MarketingCarrierCode = vredTxnRequest.transactionData.FlightDetails?.MarketingPartnerCode,
                MarketingFlightNumber = vredTxnRequest.transactionData.FlightDetails?.MarketingFlightNumber,
                PostingType = vredTxnRequest.transactionData.FlightDetails?.VAPostingTypeCode,
                OriginationAirport = vredTxnRequest.transactionData.FlightDetails?.OriginAirportcode,
                DestinationAirport = vredTxnRequest.transactionData.FlightDetails?.DestinationAirport,
                TravelDistance = vredTxnRequest.transactionData.FlightDetails?.Distance,
                BasePoints = vredTxnRequest.transactionData.CreditDetails?.FirstOrDefault(x => x.PointSubType == Constants.Application.BaseMilesType)?.CreditValue,
                ClassOfServicePoints = vredTxnRequest.transactionData.CreditDetails?.FirstOrDefault(x => x.PointSubType == Constants.Application.CabinBonusType)?.CreditValue,
                EliteTierPoints = vredTxnRequest.transactionData.CreditDetails?.FirstOrDefault(x => x.PointSubType == Constants.Application.TierBonusType)?.CreditValue,
                PointType = !string.IsNullOrEmpty(vredTxnRequest.transactionData.TransactionValue) ? Constants.Application.PointType : string.Empty,
                PromotionBonusPoints = vredTxnRequest.transactionData.CreditDetails?.FirstOrDefault(x => x.PointSubType == Constants.Application.PromotionBonusType)?.CreditValue,
                PromotionType = string.Empty,
                ClassCode = vredTxnRequest.transactionData.ClassCode,
                BookingReference = vredTxnRequest.transactionData.FlightDetails?.ExternalReservationNumber,
                CabinClass = vredTxnRequest.transactionData.FlightDetails?.CabinClass,
                FareBasis = vredTxnRequest.transactionData.FlightDetails?.FareBasis,
                LifeTimeMiles = string.Empty,
                PartnerSubType = vredTxnRequest.partnerData.PartnerSubType,
                PromotionBonusIndicator = string.Empty,
                ProductType = vredTxnRequest.productData.ProductType
            };
        }
    }
}

