using MediatR;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Transaction
{
    public class AccrualVredTransactionRepoQueryHandler : IRequestHandler<AccrualVredTransactionRepoQuery, VRedTransactionResponse>
    {
        protected readonly IMediator _mediator;
        private readonly ITransactionDBRepository _transactionDBRepository;

        public AccrualVredTransactionRepoQueryHandler(IMediator mediator, ITransactionDBRepository transactionDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _transactionDBRepository = transactionDBRepository ?? throw new ArgumentNullException(nameof(transactionDBRepository));
        }

        public async Task<VRedTransactionResponse> Handle(AccrualVredTransactionRepoQuery query, CancellationToken cancellationToken)
        {

            string transactionId = query.Request.TransactionId;

            var VRedRequestJson = JsonSerializer.Serialize(BuildAccrualTransactionPostApiRequest(query));

            await _transactionDBRepository.UpdateVRedRequestAuditInfo(transactionId, VRedRequestJson);

            return await PostVRedTransactionsAsync(BuildAccrualTransactionPostApiRequest(query));
        }

        private TransactionPostAPI BuildAccrualTransactionPostApiRequest(AccrualVredTransactionRepoQuery query)
        {
            var creditMetaDataSchema = ConfigReader.Get(Constants.Environment.CreditMetaDataSchema);

            bool manualAdjustmentFlag = false;
            bool isFlight = string.IsNullOrEmpty(string.Concat(query.Request.DestinationAirport, query.Request.BookingClass).Trim()) ? false : true;

            if (!query.Request.ProductCategory.IsNullOrWhiteSpace() && query.Request.ProductCategory.Is("Manual Adjustment"))
                manualAdjustmentFlag = true;

            DateTime createdOn = DateTime.UtcNow;

            var creditMetaData = new CreditMetaData
            {
                AccruedValueMiles = query.Request.Points,
                ActivityDatetime = Helper.Utility.ParseDateTimeZ(query.Request.ActivityDate.Value, Constants.Application.iso8601DateTimeFormat),
                ActivitySubtype = query.Request.TransactionSubType,
                ActivityType = query.Request.ActivityType,
                BookingClassCode = query.Request.BookingClass,
                BookingReference = query.Request.BookingReference,
                CancellationReason = query.Request.CancellationReason,
                CancelledBy = query.Request.CancelledBy,
                CancelledTxnId = query.Request.CancelledTransactionId,
                Comments = query.Request.Comments,
                CreatedDatetime = Helper.Utility.ParseDateTimeZ(createdOn, Constants.Application.iso8601DateTimeFormat),
                CreatedBy = query.Request.CreatedBy,
                IsFlight = isFlight,
                InsertedDatetime = Helper.Utility.ParseDateTimeZ(createdOn, Constants.Application.iso8601DateTimeFormat),
                ManualAdjustmentFlag = manualAdjustmentFlag,
                MembershipNumber = query.Request.MemberNumber.Parse<long>(0),
                NumUnits = query.Request.NoOfPassengers,
                Owner = Constants.Application.VRedTxnOwner,
                PartnerCategory = query.Request.PartnerCategory,
                PartnerCode = query.Request.PartnerCode,
                PartnerName = query.Request.PartnerName,
                PartnerSubtype = query.Request.PartnerSubType,
                PdmPartnerFilename = query.Request.PdmPartnerFileName,
                Points = query.Request.Points,
                ProductCategory = query.Request.ProductCategory,
                ProductDescription = query.Request.ProductDescription,
                ProductName = query.Request.ProductName,
                TicketIssueDatetime = !string.IsNullOrEmpty(query.Request.TicketIssueDatetime.ToString()) ? Helper.Utility.ParseDateTimeZ(query.Request.TicketIssueDatetime.Value, Constants.Application.iso8601DateTimeFormat) : null,
                PostingType = query.Request.PostingType,
                BonusMiles = query.Request.BonusMiles.Parse<int>(0),
                ActivityNumber = query.Request.ActivityNumber,
                ActivityMultiplier = query.Request.ActivityMultiplier.Parse<int>(0),
                RetroIndicatorFlag = !string.IsNullOrEmpty(query.Request.RetroIndicatorFlag) && query.Request.RetroIndicatorFlag == "Y" ? true : false,
                RetroOverrideFlag = !string.IsNullOrEmpty(query.Request.RetroOverrideFlag) && query.Request.RetroOverrideFlag == "Y" ? true : false,
                RetroOverrideReason = query.Request.RetroOverrideReason,
                TransferMembershipNumber = query.Request.TransferMemberNumber.Parse<long>(0),
                TravelMiles = query.Request.TravelDistance.Parse<int>(0),
                BasePoints = query.Request.BasePoints.Parse<int>(0),
                ClassOfServicePoints = query.Request.ClassOfServicePoints.Parse<int>(0),
                EliteTierBonusPoints = query.Request.EliteTierPoints.Parse<int>(0),
                PromotionBonusPoints = query.Request.PromotionBonusPoints.Parse<int>(0),
                ClassCode = query.Request.ClassCode,
                FareBasis = query.Request.FareBasis,
                Schema = creditMetaDataSchema,
                Sectors = isFlight ? new List<CreditSector>()
                {
                    new CreditSector
                    {
                        DestinationAirport = query.Request.DestinationAirport,
                        FlownBookingClass = query.Request.BookingClass,
                        OriginAirport = query.Request.OriginationAirport,
                        MarketingBookingClass = query.Request.MarketingBookingClass,
                        MarketingCarrierCode = query.Request.MarketingCarrierCode,
                        MarketingFlightNumber = query.Request.MarketingFlightNumber,
                        OperatingFlightNumber = query.Request.OperatingFlightNumber,
                        Points = query.Request.Points,
                        SectorCityPair = string.Concat(query.Request.OriginationAirport, query.Request.DestinationAirport)?.Trim(),
                        ServiceClass = query.Request.BookingClass,
                        TicketNumber = query.Request.TicketNumber,
                        OperatingAirlineAuthNumber = query.Request.OperatingAirlineAuthNumber,
                        CabinClass = query.Request.CabinClass
                    }
                } : null,
                StatementText = query.Request.StatementText,
                Status = query.Request.Status,
                TransactionChannelId = query.Request.TransactionChannel,
                TransactionDatetime = Helper.Utility.ParseDateTimeZ(createdOn, Constants.Application.iso8601DateTimeFormat),
                TransactionId = query.Request.TransactionId,
                TransactionStatus = Constants.Application.VRedActivityStatusProcessed,
                UpdatedBy = query.Request.UpdatedBy,
                UpdatedDatetime = Helper.Utility.ParseDateTimeZ(createdOn, Constants.Application.iso8601DateTimeFormat)
            };

            var details = Newtonsoft.Json.JsonConvert.SerializeObject(creditMetaData, new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                Formatting = Newtonsoft.Json.Formatting.Indented
            });

            var callbackUrl = ConfigReader.Get(Constants.Environment.VredTransactionAPICallbackUrl);
            var activityDate = query.Request.ActivityDate.Value.ToString(Constants.Application.TransactionAPIDateFormat);
            var transactionDate = createdOn.ToString(Constants.Application.TransactionAPIDateFormat);

            TransactionPostAPI transactionApi = new TransactionPostAPI
            {
                requestId = query.RequestId,
                partnerTransactionId = query.Request.TransactionId,
                partnerCode = Constants.Application.PartnerCode,
                partnerUserIdDomain = Constants.Application.PartnerCode,
                partnerUserId = query.Request.MemberNumber,
                dateTime = transactionDate,
                transactionPartnerCode = query.Request.PartnerCode,
                type = Constants.Application.CreditTransaction,
                points = query.Request.Points,
                activityDateTime = activityDate,
                description = query.Request.StatementText,
                callbackUrl = callbackUrl,
                details = details
            };

            return transactionApi;
        }

        private async Task<VRedTransactionResponse> PostVRedTransactionsAsync(TransactionPostAPI transactionPostRequest)
        {
            return await _mediator.Send(new VRedTransactionApiServiceQuery()
            {
                TransactionPostRequest = transactionPostRequest
            });
        }
    }
}