using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions;
using VAA.EAPI.Microservices.Loyalty.Vred.Helper;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Mapper
{
    public class CancelVredTransactionDataMapper
    {
        public static CancelVredTransactionRequestData CancelVredTransactionRequestMapper(CancelTransactionRequest cancelTransactionRequest, CreditMetaData earnTransactionDetails, DebitMetaData burnTransactionDetails)
        {
            if (cancelTransactionRequest == null) throw new ArgumentNullException(nameof(cancelTransactionRequest));
            if (earnTransactionDetails == null && burnTransactionDetails == null) throw new ArgumentNullException(nameof(earnTransactionDetails));

            if (!string.IsNullOrEmpty(earnTransactionDetails.ActivityType))
            {
                earnTransactionDetails.Status = Constants.Application.CancelStatus;
                earnTransactionDetails.CancellationReason = cancelTransactionRequest.CancellationReason;
                earnTransactionDetails.CancelledBy = cancelTransactionRequest.UpdatedBy;
                earnTransactionDetails.UpdatedBy = cancelTransactionRequest.UpdatedBy;
                earnTransactionDetails.UpdatedDatetime = Common.Helpers.DateTimeHelper.ConvertToUtc(DateTime.UtcNow.ToString(), Constants.Application.VRedDateFormat, Constants.Application.SCVDateFormat);
            }
            else
            {
                burnTransactionDetails.Status = Constants.Application.CancelStatus;
                burnTransactionDetails.CancellationReason = cancelTransactionRequest.CancellationReason;
                burnTransactionDetails.CancelledBy = cancelTransactionRequest.UpdatedBy;
                burnTransactionDetails.UpdatedBy = cancelTransactionRequest.UpdatedBy;
                burnTransactionDetails.UpdatedDatetime = Common.Helpers.DateTimeHelper.ConvertToUtc(DateTime.UtcNow.ToString(), Constants.Application.VRedDateFormat, Constants.Application.SCVDateFormat);
            }

            return new CancelVredTransactionRequestData
            {
                TransactionId = cancelTransactionRequest.TransactionId,
                MemberNumber = cancelTransactionRequest.MemberNumber,
                ActivityType = !string.IsNullOrEmpty(earnTransactionDetails.ActivityType) ? earnTransactionDetails.ActivityType : burnTransactionDetails.ActivityType,
                PartnerCode = !string.IsNullOrEmpty(earnTransactionDetails.ActivityType) ? earnTransactionDetails.PartnerCode : burnTransactionDetails.PartnerCode,
                Details = !string.IsNullOrEmpty(earnTransactionDetails.ActivityType)
                    ? JsonConvert.SerializeObject(earnTransactionDetails, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Formatting = Formatting.Indented,
                    })
                    : JsonConvert.SerializeObject(burnTransactionDetails, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Formatting = Formatting.Indented,
                    })
            };
        }

        public static CreateTransactionCommand CompensatoryBurnTransactionRequestMapper(CancelTransactionRequest cancelTransactionRequest, CreditMetaData earnTransactionDetails, string correlationId)
        {
            if (cancelTransactionRequest == null) throw new ArgumentNullException(nameof(cancelTransactionRequest));
            if (earnTransactionDetails == null) throw new ArgumentNullException(nameof(earnTransactionDetails));

            return new CreateTransactionCommand
            {
                Id = correlationId,
                Request = new CreateTransactionRequest
                {
                    TransactionId = string.IsNullOrEmpty(cancelTransactionRequest.CompensatoryTransactionId) ? $"AZ{DateTime.UtcNow.MillisecondsTimestamp()}" : cancelTransactionRequest.CompensatoryTransactionId,
                    Status = Constants.Application.VRedActivityStatusProcessed,
                    ActivityType = Constants.Application.RedemptionType,
                    TransactionSubType = Constants.Application.AccrualCancellation,
                    ActivityDate = Convert.ToDateTime(earnTransactionDetails.ActivityDatetime),
                    TransactionChannel = earnTransactionDetails.TransactionChannelId,
                    Points = (cancelTransactionRequest.CancellationReason.Is(Constants.Application.CanxReasonWithin24hr) && earnTransactionDetails.ProductName.Is(Constants.Application.VoucherPointsProductName)) ? 0 : earnTransactionDetails.Points,
                    ProductName = earnTransactionDetails.ProductName,
                    MemberNumber = cancelTransactionRequest.MemberNumber,
                    PartnerCode = earnTransactionDetails.PartnerCode,
                    CreatedBy = cancelTransactionRequest.UpdatedBy,
                    UpdatedBy = cancelTransactionRequest.UpdatedBy,
                    CancelledTransactionId = cancelTransactionRequest.TransactionId,
                    Comments = earnTransactionDetails.Comments,
                    StatementText = Constants.Application.CancelStatus + ": " + earnTransactionDetails.StatementText,
                    PartnerName = earnTransactionDetails.PartnerName,
                    PartnerCategory = earnTransactionDetails.PartnerCategory,
                    ProductCategory = earnTransactionDetails.ProductCategory,
                    TransactionSource = cancelTransactionRequest.TransactionChannel,
                    ProductDescription = earnTransactionDetails.ProductDescription,
                    BookingClass = earnTransactionDetails.BookingClassCode,
                    ActivityMultiplier = earnTransactionDetails.NumUnits.ToString(),
                    TicketIssueDatetime = !string.IsNullOrEmpty(earnTransactionDetails.TicketIssueDatetime) ? Convert.ToDateTime(earnTransactionDetails.TicketIssueDatetime) : null,
                    MarketingBookingClass = earnTransactionDetails.Sectors?.FirstOrDefault()?.MarketingBookingClass,
                    TicketNumber = earnTransactionDetails.Sectors?.FirstOrDefault()?.TicketNumber,
                    MarketingCarrierCode = earnTransactionDetails.Sectors?.FirstOrDefault()?.MarketingCarrierCode,
                    MarketingFlightNumber = earnTransactionDetails.Sectors?.FirstOrDefault()?.MarketingFlightNumber,
                    OperatingAirlineAuthNumber = earnTransactionDetails.Sectors?.FirstOrDefault()?.OperatingAirlineAuthNumber,
                    OperatingFlightNumber = earnTransactionDetails.Sectors?.FirstOrDefault()?.OperatingFlightNumber,
                    PdmPartnerFileName = earnTransactionDetails.PdmPartnerFilename,
                    OriginationAirport = earnTransactionDetails.Sectors?.FirstOrDefault()?.OriginAirport,
                    DestinationAirport = earnTransactionDetails.Sectors?.FirstOrDefault()?.DestinationAirport,
                    BookingReference = earnTransactionDetails.BookingReference,
                    PartnerSubType = earnTransactionDetails.PartnerSubtype,
                    PostingType = earnTransactionDetails.PostingType,
                    ActivityNumber = earnTransactionDetails.ActivityNumber,
                    RetroIndicatorFlag = earnTransactionDetails.RetroIndicatorFlag.ToString(),
                    TravelDistance = earnTransactionDetails.TravelMiles.ToString(),
                    TransferMemberNumber = earnTransactionDetails.TransferMembershipNumber.ToString(),
                    ClassCode = earnTransactionDetails.ClassCode,
                    CabinClass = earnTransactionDetails.Sectors?.FirstOrDefault()?.CabinClass,
                    FareBasis = earnTransactionDetails.FareBasis,
                    RetroOverrideFlag = earnTransactionDetails.RetroOverrideFlag.ToString(),
                    RetroOverrideReason = earnTransactionDetails.RetroOverrideReason
                }
            };
        }

        public static CreateTransactionCommand CompensatoryEarnTransactionRequestMapper(CancelTransactionRequest cancelTransactionRequest, DebitMetaData burnTransactionDetails, string correlationId)
        {
            if (cancelTransactionRequest == null) throw new ArgumentNullException(nameof(cancelTransactionRequest));
            if (burnTransactionDetails == null) throw new ArgumentNullException(nameof(burnTransactionDetails));

            return new CreateTransactionCommand
            {
                Id = correlationId,
                Request = new CreateTransactionRequest
                {
                    TransactionId = string.IsNullOrEmpty(cancelTransactionRequest.CompensatoryTransactionId) ? $"AZ{DateTime.UtcNow.MillisecondsTimestamp()}" : cancelTransactionRequest.CompensatoryTransactionId,
                    Status = Constants.Application.VRedActivityStatusProcessed,
                    ActivityType = Constants.Application.AccrualType,
                    TransactionSubType = Constants.Application.RedemptionCancellation,
                    ActivityDate = Convert.ToDateTime(burnTransactionDetails.ActivityDatetime),
                    TransactionChannel = burnTransactionDetails.TransactionChannelId,
                    Points = (cancelTransactionRequest.CancellationReason.Is(Constants.Application.CanxReasonWithin24hr) || cancelTransactionRequest.CancellationReason.Is(Constants.Application.CanxReasonNoRefundOfPoints)) ? 0 : burnTransactionDetails.Points,
                    ProductName = burnTransactionDetails.ProductName,
                    MemberNumber = cancelTransactionRequest.MemberNumber,
                    PartnerCode = burnTransactionDetails.PartnerCode,
                    CreatedBy = cancelTransactionRequest.UpdatedBy,
                    UpdatedBy = cancelTransactionRequest.UpdatedBy,
                    CancelledTransactionId = cancelTransactionRequest.TransactionId,
                    Comments = burnTransactionDetails.Comments,
                    StatementText = Constants.Application.CancelStatus + ": " + burnTransactionDetails.StatementText,
                    PartnerName = burnTransactionDetails.PartnerName,
                    PartnerCategory = burnTransactionDetails.PartnerCategory,
                    ProductCategory = burnTransactionDetails.ProductCategory,
                    TransactionSource = cancelTransactionRequest.TransactionChannel,
                    ProductDescription = burnTransactionDetails.ProductDescription,
                    BookingClass = burnTransactionDetails.BookingClassCode,
                    ActivityMultiplier = burnTransactionDetails.NumUnits.ToString(),
                    TicketNumber = burnTransactionDetails.Sectors?.FirstOrDefault()?.TicketNumber,
                    OperatingFlightNumber = burnTransactionDetails.Sectors?.FirstOrDefault()?.OperatingFlightNumber,
                    PdmPartnerFileName = burnTransactionDetails.PdmPartnerFilename,
                    OriginationAirport = burnTransactionDetails.Sectors?.FirstOrDefault()?.OriginAirport,
                    DestinationAirport = burnTransactionDetails.Sectors?.FirstOrDefault()?.DestinationAirport,
                    BookingReference = burnTransactionDetails.BookingReference,
                    PartnerSubType = burnTransactionDetails.PartnerSubtype,
                    ActivityNumber = burnTransactionDetails.ActivityNumber,
                    TransferMemberNumber = burnTransactionDetails.TransferMembershipNumber.ToString()
                }
            };
        }
    }
}
