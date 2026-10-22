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
    public class RedemptionVredTransactionRepoQueryHandler : IRequestHandler<RedemptionVredTransactionRepoQuery, VRedTransactionResponse>
    {
        protected readonly IMediator _mediator;
        private readonly ITransactionDBRepository _transactionDBRepository;

        public RedemptionVredTransactionRepoQueryHandler(IMediator mediator, ITransactionDBRepository transactionDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _transactionDBRepository = transactionDBRepository ?? throw new ArgumentNullException(nameof(transactionDBRepository));
        }

        public async Task<VRedTransactionResponse> Handle(RedemptionVredTransactionRepoQuery query, CancellationToken cancellationToken)
        {
            string transactionId = query.Request.TransactionId;

            var VRedRequestJson = JsonSerializer.Serialize(BuildRedemptionTransactionPostApiRequest(query));

            await _transactionDBRepository.UpdateVRedRequestAuditInfo(transactionId, VRedRequestJson);

            return await PostVRedTransactionsAsync(BuildRedemptionTransactionPostApiRequest(query));
        }

        private TransactionPostAPI BuildRedemptionTransactionPostApiRequest(RedemptionVredTransactionRepoQuery query)
        {
            var debitMetaDataSchema = ConfigReader.Get(Constants.Environment.DebitMetaDataSchema);

            bool manualAdjustmentFlag = false;
            if (!query.Request.ProductCategory.IsNullOrWhiteSpace() && query.Request.ProductCategory.Is("Manual Adjustment"))
                manualAdjustmentFlag = true;

            DateTime createdOn = DateTime.UtcNow;
            bool isFlight = string.IsNullOrEmpty(string.Concat(query.Request.DestinationAirport, query.Request.BookingClass).Trim()) ? false : true;

            var debitMetaData = new DebitMetaData
            {
                ActivityDatetime = Helper.Utility.ParseDateTimeZ(query.Request.ActivityDate.Value, Constants.Application.iso8601DateTimeFormat),
                ActivitySubtype = query.Request.TransactionSubType,
                ActivityType = query.Request.ActivityType,
                CancellationReason = query.Request.CancellationReason,
                CancelledBy = query.Request.CancelledBy,
                CancelledTxnId = query.Request.CancelledTransactionId,
                Comments = query.Request.Comments,
                CreatedDatetime = Helper.Utility.ParseDateTimeZ(createdOn, Constants.Application.iso8601DateTimeFormat),
                CreatedBy = query.Request.CreatedBy,
                InsertedDatetime = Helper.Utility.ParseDateTimeZ(createdOn, Constants.Application.iso8601DateTimeFormat),
                ManualAdjustmentFlag = manualAdjustmentFlag,
                MembershipNumber = query.Request.MemberNumber.Parse<long>(0),
                Owner = Constants.Application.VRedTxnOwner,
                PartnerCategory = query.Request.PartnerCategory,
                PartnerCode = query.Request.PartnerCode,
                PartnerName = query.Request.PartnerName,
                Points = query.Request.Points,
                ProductDescription = query.Request.ProductDescription,
                ProductCategory = query.Request.ProductCategory,
                ProductName = query.Request.ProductName,
                Schema = debitMetaDataSchema,
                StatementText = query.Request.StatementText,
                Status = query.Request.Status,
                TransactionChannelId = query.Request.TransactionChannel,
                TransactionDatetime = Helper.Utility.ParseDateTimeZ(createdOn, Constants.Application.iso8601DateTimeFormat),
                TransactionId = query.Request.TransactionId,
                TransactionStatus = Constants.Application.VRedActivityStatusProcessed,
                UpdatedBy = query.Request.UpdatedBy,
                UpdatedDatetime = Helper.Utility.ParseDateTimeZ(createdOn, Constants.Application.iso8601DateTimeFormat),
                BookingClassCode = query.Request.BookingClass,
                NumUnits = query.Request.ActivityMultiplier.Parse<int>(0),
                BookingReference = query.Request.BookingReference,
                PdmPartnerFilename = query.Request.PdmPartnerFileName,
                PartnerSubtype = query.Request.PartnerSubType,
                ActivityNumber = query.Request.ActivityNumber,
                TransferMembershipNumber = query.Request.TransferMemberNumber.Parse<long>(0),
                RetroIndicatorFlag = !string.IsNullOrEmpty(query.Request.RetroIndicatorFlag) && query.Request.RetroIndicatorFlag == "Y" ? true : false,
                RetroOverrideFlag = !string.IsNullOrEmpty(query.Request.RetroOverrideFlag) && query.Request.RetroOverrideFlag == "Y" ? true : false,
                RetroOverrideReason = query.Request.RetroOverrideReason,
                ClassCode = query.Request.ClassCode,
                FareBasis = query.Request.FareBasis,
                VoucherDescription = query.Request.VoucherDescription,
                VoucherNumber = query.Request.VoucherNumber,
                VoucherStatus = query.Request.VoucherStatus,
                VoucherType = query.Request.VoucherType,
                VoucherExpiryDatetime = !string.IsNullOrEmpty(query.Request.VoucherExpiryDate.ToString()) ? Helper.Utility.ParseDateTimeZ(query.Request.VoucherExpiryDate.Value, Constants.Application.iso8601DateTimeFormat) : null,
                VoucherIssueDatetime = !string.IsNullOrEmpty(query.Request.VoucherIssueDate.ToString()) ? Helper.Utility.ParseDateTimeZ(query.Request.VoucherIssueDate.Value, Constants.Application.iso8601DateTimeFormat) : null,
                TicketIssueDatetime = !string.IsNullOrEmpty(query.Request.TicketIssueDatetime.ToString()) ? Helper.Utility.ParseDateTimeZ(query.Request.TicketIssueDatetime.Value, Constants.Application.iso8601DateTimeFormat) : null,
                Sectors = isFlight ? new List<DebitSector>()
                {
                    new DebitSector
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
                        CabinClass = query.Request.CabinClass
                    }
                } : null,
            };

            var details = Newtonsoft.Json.JsonConvert.SerializeObject(debitMetaData, new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                Formatting = Newtonsoft.Json.Formatting.Indented
            });

            var callbackUrl = ConfigReader.Get(Constants.Environment.VredTransactionAPICallbackUrl);
            var activityDate = query.Request.ActivityDate.Value.ToString(Constants.Application.TransactionAPIDateFormat);
            var transactionDate = createdOn.ToString(Constants.Application.TransactionAPIDateFormat);

            TransactionPostAPI transactionPostApi = new TransactionPostAPI
            {
                requestId = query.RequestId,
                partnerTransactionId = query.Request.TransactionId,
                partnerCode = Constants.Application.PartnerCode,
                partnerUserIdDomain = Constants.Application.PartnerCode,
                partnerUserId = query.Request.MemberNumber,
                dateTime = transactionDate,
                transactionPartnerCode = query.Request.PartnerCode,
                type = Constants.Application.DebitTransaction,
                points = query.Request.Points,
                activityDateTime = activityDate,
                description = query.Request.StatementText,
                callbackUrl = callbackUrl,
                details = details
            };

            return transactionPostApi;
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