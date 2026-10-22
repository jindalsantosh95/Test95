using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.TxnStatementText;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mappers
{
    public class TransactionMapper
    {
        public static TransactionRequest MapToVchrAccrualTransactionRequest(CreateTransactionMutationRequest transactionMutationRequest, string productId, string partnerId)
        {
            if (transactionMutationRequest is null) throw new ArgumentNullException(nameof(transactionMutationRequest));
            if (productId is null) throw new ArgumentNullException(nameof(productId));

            return new TransactionRequest()
            {
                TransactionDocument = new Transaction()
                {
                    TransactionId = Utility.GetId(),
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = transactionMutationRequest.CreatedBy,
                    LastUpdatedOn = DateTime.UtcNow,
                    LastUpdatedBy = transactionMutationRequest.CreatedBy,
                    CustomerId = transactionMutationRequest.CustomerId,
                    Comments = transactionMutationRequest.Comments,
                    MemberNumber = transactionMutationRequest.MemberNumber,
                    TransactionValue = transactionMutationRequest.Points.ToString(),
                    PartnerId = partnerId,
                    TransactionDate = DateTime.UtcNow,
                    ProductId = productId,
                    StatusCode = Common.Constants.Application.TxnProcessingStatus,
                    TransactionSubType = Common.Constants.Application.TxnSubType,
                    TransactionChannel = transactionMutationRequest.TransactionSource,
                    ActivityDate = transactionMutationRequest.ActivityDate,
                    TransactionType = Common.Constants.Application.AccrualTransactionType,
                    AwardId = transactionMutationRequest.VoucherDetails?.VoucherId,
                    PointOverrideFlag = transactionMutationRequest.PointOverrideFlag,
                    FlightDetails = new FlightDetails()
                    {
                        BookingClass = transactionMutationRequest.FlightDetails?.BookingClass,
                        OriginAirportcode = transactionMutationRequest.FlightDetails?.OriginationAirport,
                        DestinationAirport = transactionMutationRequest.FlightDetails?.DestinationAirport,
                        ExternalReservationNumber = transactionMutationRequest.FlightDetails?.BookingReference,
                        Ticketnumber = transactionMutationRequest.FlightDetails?.TicketNumber,
                        NumberOfSegments = transactionMutationRequest.NoOfSegments.ToString(),
                        Quantity = transactionMutationRequest.NoOfPassengers.ToString()
                    },
                    CreditDetails = new List<CreditDetails>()
                    {
                        new CreditDetails()
                        {
                            CreatedBy =  transactionMutationRequest.CreatedBy,
                            LastUpdatedBy = transactionMutationRequest.CreatedBy,
                            CreditValue = transactionMutationRequest.Points.ToString(),
                            PointType = Common.Constants.Application.MilesType,
                            PointSubType = Common.Constants.Application.BaseMilesType,
                            QualifyingFlag = Common.Constants.Application.MilesQualifyingFlag
                        }
                    }
                }
            };
        }


        public static VredSyncTransactionRequest MapToVredTransactionRequest(CreateTransactionMutationRequest requestData, Transaction transactionData, PartnerProfileDetails partnerData, ProductDetails productData, Models.Award awardData, ProductDetails awardProductdata)
        {
            if (transactionData is null) throw new ArgumentNullException(nameof(transactionData));
            if (requestData is null) throw new ArgumentNullException(nameof(requestData));
            if (partnerData is null) throw new ArgumentNullException(nameof(partnerData));
            if (productData is null) throw new ArgumentNullException(nameof(productData));
            if (awardData?.AwardId is not null && awardProductdata is null) throw new ArgumentNullException(nameof(awardProductdata));


            return new VredSyncTransactionRequest()
            {
                CustomerId = transactionData.CustomerId,
                TransactionId = transactionData.TransactionId,
                PartnerCode = partnerData.PartnerCode,
                PartnerName = partnerData.Name,
                MemberNumber = transactionData.MemberNumber,
                TransactionDate = transactionData.TransactionDate,
                ActivityType = transactionData.TransactionType,
                Points = Convert.ToInt32(transactionData.TransactionValue),
                ActivityDate = transactionData.ActivityDate,
                StatementText = transactionData.StatementText,
                CreatedBy = transactionData.CreatedBy,
                Created = transactionData.CreatedOn,
                Updated = transactionData.LastUpdatedOn,
                UpdatedBy = transactionData.LastUpdatedBy,
                ParentTransactionId = transactionData.ParentTransactionId,
                TransactionChannel = transactionData.TransactionChannel,
                Status = transactionData.StatusCode,
                BookingClass = transactionData.FlightDetails?.BookingClass,
                TransactionNumber = transactionData.TransactionId,
                BonusMiles = transactionData.FinancialCreditDetails?.BonusMiles,
                ActivityNumber = transactionData.ActivityNumber,
                OperatingFlightNumber = transactionData.FlightDetails?.OperatingFlightNumber,
                NoofPassengers = Convert.ToInt32(transactionData.FlightDetails?.Quantity),
                NoOfSegments = Convert.ToInt32(transactionData.FlightDetails?.NumberOfSegments),
                ProductName = productData.ProductName,
                TicketNumber = transactionData.FlightDetails?.Ticketnumber,
                TicketIssueDatetime = transactionData.FlightDetails?.BookingDate,
                OperatingAirlineAuthNumber = transactionData.FlightDetails?.OperatingAirlineAuthNumber,
                TransactionSubType = transactionData.TransactionSubType,
                CancellationReason = transactionData.CancellationReasonCode,
                Comments = transactionData.Comments,
                CancelledTransactionId = transactionData.CancelledTransactionId,
                VoucherNumber = awardData?.AwardNumber,
                VoucherType = awardProductdata?.ProductName,
                VoucherStatus = requestData.VoucherDetails?.VoucherUsage,
                VoucherDescription = awardProductdata?.Description,
                VoucherIssueDate = awardData?.StartDate,
                PartnerCategory = partnerData.PartnerType,
                ProductCategory = productData.ProductCategory,
                ActivityMultiplier = transactionData.ActivityMultiplier,
                VAPDMDocNum = transactionData.PDMDocNum,
                PANNumber = transactionData.FinancialCreditDetails?.PANNumber,
                PDMFileType = transactionData.PDMFileType,
                GiftAdjustments = transactionData.FinancialCreditDetails?.GiftMiles,
                NonGiftAdjustments = transactionData.FinancialCreditDetails?.NonGiftAdjustments,
                PDMPartnerFileName = string.Empty,
                CancelledBy = transactionData.CancelledBy,
                ProductDescription = productData.Description,
                RetroIndicatorFlag = transactionData.FlightDetails?.RetroFlag,
                RetroOverrideFlag = transactionData.FlightDetails?.RetroOverrideFlag,
                RetroOverrideReason = transactionData.FlightDetails?.RetroOverrideReason,
                StandardMiles = transactionData.FinancialCreditDetails?.StandardMiles,
                TransferMemberNumber = transactionData.TransferMemberNumber,
                VAPDMRecordId = transactionData.PDMRecordId,
                VoucherExpiryDate = awardData?.ExpiryDate,
                SegmentBookingClass = transactionData.FlightDetails?.PassengerDetails?.ElementAtOrDefault(0)?.SegmentDetails?.ElementAtOrDefault(0)?.BookingClass,
                MarketingBookingClass = transactionData.FlightDetails?.MarketingBookingClass,
                MarketingCarrierCode = transactionData.FlightDetails?.MarketingPartnerCode,
                MarketingFlightNumber = transactionData.FlightDetails?.MarketingFlightNumber,
                SegmentFlightNumber = transactionData.FlightDetails?.PassengerDetails?.ElementAtOrDefault(0)?.SegmentDetails?.ElementAtOrDefault(0)?.OperatingFlightNumber,
                AuthorizationNumber = transactionData.TransactionIntegrationId,
                PostingType = transactionData.FlightDetails?.VAPostingTypeCode,
                OriginationAirport = transactionData.FlightDetails?.OriginAirportcode,
                DestinationAirport = transactionData.FlightDetails?.DestinationAirport,
                TravelDistance = transactionData.FlightDetails?.Distance,
                BasePoints = transactionData.CreditDetails?.ToList().FirstOrDefault(x => x.PointSubType == Common.Constants.Application.BaseMilesType)?.CreditValue,
                ClassOfServicePoints = transactionData.CreditDetails?.ToList().FirstOrDefault(x => x.PointSubType == Common.Constants.Application.CabinBonusType)?.CreditValue,
                EliteTierPoints = transactionData.CreditDetails?.ToList().FirstOrDefault(x => x.PointSubType == Common.Constants.Application.TierBonusType)?.CreditValue,
                PointType = !string.IsNullOrEmpty(transactionData.TransactionValue) ? Common.Constants.Application.PointType : string.Empty,
                PromotionBonusPoints = transactionData.CreditDetails?.ToList().FirstOrDefault(x => x.PointSubType == Common.Constants.Application.PromotionBonusType)?.CreditValue,
                PromotionType = string.Empty,
                ClassCode = transactionData.ClassCode,
                BookingReference = transactionData.FlightDetails?.ExternalReservationNumber,
                CabinClass = transactionData.FlightDetails?.CabinClass,
                FareBasis = transactionData.FlightDetails?.FareBasis,
                LifeTimeMiles = string.Empty,
                PartnerSubType = partnerData.PartnerSubtype,
                PromotionBonusIndicator = string.Empty
            };
        }

        public static CreateTransactionMutationResponse MapToTransactionResponse(Transaction transactionData, IEnumerable<ErrorDetails> errors, string source)
        {
            if (errors is not null)
            {
                return new CreateTransactionMutationResponse()
                {
                    Errors = new List<ErrorDetails>()
                    {
                         new ErrorDetails()
                         {
                             Code = errors?.FirstOrDefault()?.Code,
                             Message= $"{source} failure - {errors?.FirstOrDefault()?.Message}",
                             DeveloperMessage= errors?.FirstOrDefault()?.DeveloperMessage
                         }
                    }
                };
            }

            return new CreateTransactionMutationResponse()
            {
                TransactionId = transactionData.TransactionId,
                TransactionStatus = transactionData.StatusCode
            };
        }

        public static TransactionStatementTextRequest MapToTransactionStatementRequest(Transaction transactionData, PartnerProfileDetails partnerData, ProductDetails productData, ProductDetails awardProductdata, CustomerCustMS customerData)
        {
            if (transactionData is null) throw new ArgumentNullException(nameof(transactionData));
            if (partnerData is null) throw new ArgumentNullException(nameof(partnerData));
            if (productData is null) throw new ArgumentNullException(nameof(productData));

            return new TransactionStatementTextRequest()
            {
                TransactionType = transactionData.TransactionType,
                ProductName = productData.ProductName,
                ProductDescription = productData.Description,
                PartnerName = partnerData.Name,
                ActivityNumber = transactionData.ActivityNumber,
                PartnerCode = partnerData.PartnerCode,
                ExternalReservationNumber = transactionData.FlightDetails?.ExternalReservationNumber,
                Quantity = transactionData.FlightDetails?.Quantity,
                OriginAirport = transactionData.FlightDetails?.OriginAirportcode,
                DestinationAirport = transactionData.FlightDetails?.DestinationAirport,
                TransactionSubType = transactionData.TransactionSubType,
                MemberType = customerData?.MemberAccountType,
                MemberMediaCode = string.Empty,
                MarketingCarrierCode = transactionData.FlightDetails?.MarketingPartnerCode,
                CabinClass = transactionData.FlightDetails?.TravelClass,
                VACodeShareFlag = transactionData.FlightDetails?.CodeShareFlag,
                MergedMemberNumber = transactionData.TransferMemberNumber,
                StatementText = transactionData.StatementText.IsNullOrEmpty() ? string.Empty : transactionData.StatementText,
                ClassCode = transactionData.ClassCode,
                VoucherName = awardProductdata?.ProductName
            };
        }

        public static TransactionRequest MapToCancelTransactionRequest(CancelTransactionDetails cancelTxnDetails, string partnerName, CancelTransactionMutationRequest cancelRequest, IEnumerable<ProductDetails> productDetails, string canxReason, Award awardDetails)
        {
            if (cancelTxnDetails is null) throw new ArgumentNullException(nameof(cancelTxnDetails));
            if (partnerName is null) throw new ArgumentNullException(nameof(partnerName));

            return new TransactionRequest()
            {
                ProductName = string.IsNullOrEmpty(cancelTxnDetails.ProductName) ? productDetails.FirstOrDefault(p => p.RowId == cancelTxnDetails.ProductId).ProductName : cancelTxnDetails.ProductName,
                PartnerName = partnerName,
                TransactionDocument = new()
                {
                    CustomerId = cancelRequest.CustomerId,
                    MemberNumber = cancelRequest.MemberNumber,
                    TransactionId = cancelTxnDetails.TransactionId,
                    CancellationReasonCode = canxReason,
                    CancelledBy = cancelRequest.InitiatedBy,
                    LastUpdatedOn = DateTime.UtcNow,
                    TransactionChannel = cancelRequest.TransactionSource,
                    AwardName = !string.IsNullOrEmpty(awardDetails?.ProductId) ? productDetails.FirstOrDefault(p => p.RowId == awardDetails.ProductId).ProductName : null,
                }
            };
        }
        public static VredCancelTransactionRequest MapToVredCancelRequest(CancelTransactionMutationRequest cancelRequest, CancelTransactionDetails cancelTxnDetails, string compensatoryId, string canxReason)
        {
            if (cancelTxnDetails is null) throw new ArgumentNullException(nameof(cancelTxnDetails));
            if (cancelRequest is null) throw new ArgumentNullException(nameof(cancelRequest));

            return new VredCancelTransactionRequest()
            {
                TransactionId = cancelTxnDetails.TransactionId,
                TransactionChannel = cancelRequest.TransactionSource,
                CancellationReason = canxReason,
                UpdatedBy = cancelRequest.InitiatedBy,
                MemberNumber = cancelRequest.MemberNumber,
                CompensatoryTransactionId = compensatoryId
            };
        }
    }

}