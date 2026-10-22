using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction
{

    public class TransactionCreateEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public TransactionCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }
    public class TransactionCreateEventHandler : TransactionBaseEventHandler, IRequestHandler<TransactionCreateEvent, Result<Unit>>
    {
        public TransactionCreateEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        { }

        public Task<Result<Unit>> Handle(TransactionCreateEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }
        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            TransactionCommandBase createEvent = Json.DeserializeObject<TransactionCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new TransactionCreateValidator().Validate(createEvent.TransactionDocument);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Transaction create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            var transactionDocs = (await TransactionCommandBase.GetListOfTransactions(_mediator, createEvent.TransactionDocument.CustomerId, cancellationToken))
                                                        .Where(n => n.CustomerId == createEvent.TransactionDocument.CustomerId);
            if (transactionDocs is null)
                throw new RequestValidationException(ErrorCodes.CustomerIdNotFoundMessage);

            //Check for Duplicate Transaction - Not applicable to Voucher Accrual
            if ((createEvent.TransactionDocument.TransactionType.Is(Constants.Application.AccrualTransactionType) && createEvent.TransactionDocument.AwardId.IsNullOrWhiteSpace() && !createEvent.TransactionDocument.TransactionSubType.Is(Constants.Application.RedemptionCancellation)) || (createEvent.TransactionDocument.TransactionType.Is(Constants.Application.RedemptionTransactionType) && !createEvent.TransactionDocument.TransactionSubType.Is(Constants.Application.AccrualCancellation)))
            {
                var dupeTransaction = transactionDocs.Where(n => n.ProductId == createEvent.TransactionDocument.ProductId && n.StatusCode == createEvent.TransactionDocument.StatusCode && Convert.ToDateTime(n.ActivityDate).Date == Convert.ToDateTime(createEvent.TransactionDocument.ActivityDate).Date && n.FlightDetails.OriginAirportcode == createEvent.TransactionDocument.FlightDetails.OriginAirportcode && n.FlightDetails.DestinationAirport == createEvent.TransactionDocument.FlightDetails.DestinationAirport && n.TransactionType == createEvent.TransactionDocument.TransactionType);

                string awardId = createEvent.TransactionDocument.AwardId;
                bool isRewardRedemptionDuplicate = createEvent.TransactionDocument.TransactionType.Is(Constants.Application.AccrualTransactionType) || dupeTransaction.Any(n => n.AwardId.IsNullOrWhiteSpace() ? awardId.IsNullOrWhiteSpace() : n.AwardId == awardId);

                if (dupeTransaction.Any() && isRewardRedemptionDuplicate)
                    throw new RequestValidationException(ErrorCodes.DuplicateTransaction);
            }

            if (createEvent.TransactionDocument.TransactionType.Is(Constants.Application.AccrualTransactionType) && !createEvent.TransactionDocument.AwardId.IsNullOrWhiteSpace() && !createEvent.TransactionDocument.TransactionSubType.Is(Constants.Application.RedemptionCancellation) && createEvent.TransactionDocument.StatusCode.Is(Constants.Application.StatusCode))
            {
                var dupeVoucherAcrlTxn = transactionDocs.Where(n => n.ProductId == createEvent.TransactionDocument.ProductId && n.StatusCode == createEvent.TransactionDocument.StatusCode && n.AwardId == createEvent.TransactionDocument.AwardId && n.TransactionType == createEvent.TransactionDocument.TransactionType).FirstOrDefault();

                if (dupeVoucherAcrlTxn is not null)
                {
                    var error = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.DuplicateTransactionVocuherAccrual), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);

                    throw new RequestValidationException(ErrorCodes.DuplicateTransactionVocuherAccrual, new Exception(error.FirstOrDefault().Message.Replace("{TransactionId}", dupeVoucherAcrlTxn.TransactionId).Replace("{ActivityDate}", DateTimeHelper.ParseDateFormat(dupeVoucherAcrlTxn.ActivityDate)?.ToString(Constants.Application.DateMonthYearFormat))));
                }
            }

            //data mapping
            TransactionDTO dto = (TransactionDTO)createEvent.TransactionDocument;
            dto.Id = transactionEvent.Id;

            //update VSFlownMiles for Accrual VS Flight Transactions
            if (createEvent.TransactionDocument.TransactionType.Is(Constants.Application.AccrualTransactionType) && (createEvent.TransactionDocument.FlightDetails?.PassengerDetails?.FirstOrDefault()?.SegmentDetails?.FirstOrDefault()?.PartnerCode?.Is(Constants.Application.VSFlightPartnerCode) ?? false) && (createEvent.TransactionDocument.FlightDetails?.Distance.IsNullOrEmpty() == false) && createEvent.ProductName.Is(Constants.Application.FlightProductName) && (createEvent.PartnerName?.Replace(" ", "").Is(Constants.Application.VSFlightPartnerName.Replace(" ", "")) ?? false))
            {
                var existingFCMDoc = (await FlyingClubMemberCommandBase.GetListOfFlyingClubMembers(_mediator, createEvent.TransactionDocument.CustomerId, cancellationToken))
                                    .FirstOrDefault(n => n.CustomerId == createEvent.TransactionDocument.CustomerId);

                FlyingClubMemberPatchEvent eventData = FlyingClubMemberUpdateEventDataMapper.MapVSFlownMilesEventData(existingFCMDoc, createEvent.TransactionDocument.FlightDetails.Distance, createEvent.TransactionDocument.CustomerId);

                var cloudEvent = new CloudEvent()
                {
                    Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateFlyingClubMemberEvent.ToString())),
                    Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                    Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateFlyingClubMemberEvent.ToString())),
                    Time = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                    Specversion = Constants.Application.SpecVersion,
                    Data = eventData
                };

                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                try
                {
                    await _mediator.Publish(cloudEvent);
                }

                catch
                {
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                    throw;
                }
            }

            return new CosmosDocumentEvent(dto, transactionEvent.EventType);
        }
    }
}
