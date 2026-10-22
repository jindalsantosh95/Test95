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
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction
{
    public class TransactionCancelEventHandler : IRequestHandler<TransactionCancelEvent, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransactionCancelEventHandler> _logger;

        public TransactionCancelEventHandler(IMediator mediator, ILogger<TransactionCancelEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(TransactionCancelEvent cancelTxnEvent, CancellationToken cancellationToken)
        {
            TransactionDTO cancelTransactionResponse = new TransactionDTO();
            Result<Response<CosmosDocumentEvent>> result = null;

            try
            {
                TransactionCommandBase cancelEvent = Json.DeserializeObject<TransactionCommandBase>((JToken)cancelTxnEvent.Data);

                var validationResult = new TransactionCancelValidator().Validate(cancelEvent.TransactionDocument);

                if (!validationResult.IsValid)
                    throw new RequestValidationException("Transaction create request contains invalid data or missing mandatory fields.", validationResult.Errors);

                var memberTransactions = await TransactionCommandBase.GetListOfTransactions(_mediator, cancelEvent.TransactionDocument.CustomerId, cancellationToken);

                if (memberTransactions is null)
                    throw new RequestValidationException(ErrorCodes.CustomerIdNotFoundMessage);

                var existingTransaction = memberTransactions.FirstOrDefault(n => n.TransactionId == cancelEvent.TransactionDocument.TransactionId);

                if (existingTransaction is null)
                    throw new RequestValidationException(ErrorCodes.TransactionIdNotFoundMessage);

                if (existingTransaction.StatusCode != Constants.Application.StatusCode || !string.IsNullOrEmpty(existingTransaction.ParentTransactionId) || !string.IsNullOrEmpty(existingTransaction.CancelledTransactionId))
                    throw new RequestValidationException(ErrorCodes.InvalidCancellationTransaction);

                if ((existingTransaction.TransactionType == Constants.Application.AccrualTransactionType && Constants.Application.AccrualCancellationReasons.Contains(cancelEvent.TransactionDocument.CancellationReasonCode))
                     || (existingTransaction.TransactionType == Constants.Application.RedemptionTransactionType && Constants.Application.RedemptionCancellationReasons.Contains(cancelEvent.TransactionDocument.CancellationReasonCode)))
                {
                    //update main/Parent Txn
                    var updateResponse = await UpdateCancelledTransaction(existingTransaction, cancelEvent.TransactionDocument);

                    //update FCM Doc for flight product
                    if (cancelEvent.ProductName.Equals(Constants.Application.FlightProductName, StringComparison.CurrentCultureIgnoreCase) && cancelEvent.PartnerName.Equals(Constants.Application.VSFlightPartnerName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var existingFCMDoc = (await FlyingClubMemberCommandBase.GetListOfFlyingClubMembers(_mediator, cancelEvent.TransactionDocument.CustomerId, cancellationToken))
                                                            .FirstOrDefault(n => n.CustomerId == cancelEvent.TransactionDocument.CustomerId);

                        if (existingFCMDoc != null)
                        {
                            await UpdateFCMDoc(existingFCMDoc, cancelEvent.TransactionDocument, existingTransaction);
                        }
                    }

                    //Create compensatory Txns
                    if (!cancelEvent.ProductName.Equals(Constants.Application.VoucherProductName, StringComparison.CurrentCultureIgnoreCase) && !cancelEvent.ProductName.Equals(Constants.Application.VoucherGiftProductName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(existingTransaction.AwardId) && existingTransaction.TransactionType == Constants.Application.RedemptionTransactionType && cancelEvent.TransactionDocument.CancellationReasonCode != "Within 24h of travel" && cancelEvent.TransactionDocument.CancellationReasonCode != "PPM - No refund of points")
                        {
                            var existingAward = (await AwardCommandBase.GetListOfAwards(_mediator, cancelEvent.TransactionDocument.CustomerId, cancellationToken))
                                                            .FirstOrDefault(n => n.AwardId == existingTransaction.AwardId);
                            if (existingAward != null)
                            {
                                //Update Award
                                await UpdateAward(existingTransaction, existingAward.Id, cancelEvent.TransactionDocument.AwardName);
                            }
                        }

                        cancelTransactionResponse = await CreateCompensatoryTransaction(existingTransaction, cancelEvent.TransactionDocument, cancelEvent.ProductName);
                    }
                }

                result = Result.Success(
                            new Response<CosmosDocumentEvent>()
                            {
                                Status = Status.OK,
                                Value = new CosmosDocumentEvent { Document = cancelTransactionResponse }
                            });
            }
            catch (RequestValidationException rvex)
            {
                result = Result.Success(
                    new Response<CosmosDocumentEvent>()
                    {
                        Status = Status.BusinessException,
                        Errors = (await TransactionBaseEventHandler.MapValidationErrors(rvex)).ToList()
                    }
                    );
            }
            catch (Exception ex)
            {
                result = Result.Failure<Response<CosmosDocumentEvent>>(ex.Message.ToString());
            }

            // Store response to store 
            await _mediator.Send(new ServiceResponseInsertCommand { Id = cancelTxnEvent.Id, JsonResponseData = Json.Serialize(result), Status = result.IsFailure ? ServiceResponseProcessStatus.Error : ServiceResponseProcessStatus.Completed, Type = Constants.Application.CommandResponseStatusKey });

            return Unit.Value;
        }

        private async Task UpdateFCMDoc(Domain.FlyingClubMember.FlyingClubMember existingFCMDoc, TransactionDocument requestTransactionDocument, TransactionDocument existingTxn)
        {
            if (existingFCMDoc.MemberAttributes.Any(x => x.AttributeName.Trim().Is(Constants.Application.FlownMiles)))
            {
                Domain.FlyingClubMember.MemberAttribute memberAttributeUpsert = existingFCMDoc.MemberAttributes?.FirstOrDefault(x => x.AttributeName.Trim().Equals(Constants.Application.FlownMiles, StringComparison.InvariantCultureIgnoreCase));
                memberAttributeUpsert.Value = (float.Parse(memberAttributeUpsert.Value) - float.Parse(existingTxn.FlightDetails.Distance)).ToString();
                memberAttributeUpsert.LastUpdatedOn = DateTime.UtcNow;
                memberAttributeUpsert.LastUpdatedBy = requestTransactionDocument.CancelledBy;

                FlyingClubMemberCommandBase fcMemberData = new()
                {
                    FlyingClubMemberDocument = new()
                    {
                        CustomerId = requestTransactionDocument.CustomerId,
                        LastUpdatedBy = requestTransactionDocument.CancelledBy,
                        LastUpdatedOn = DateTime.UtcNow,
                        MemberAttributes = [memberAttributeUpsert]
                    },
                    CustomerId = requestTransactionDocument.CustomerId
                };

                var cloudEvent = FlyingClubMemberPatchCommandToEventMapper.CommandToEventMapper(fcMemberData);

                await PublishEvent(cloudEvent);
            }
        }

        private async Task<TransactionDTO> UpdateCancelledTransaction(TransactionDocument existingTransaction, TransactionDocument requestTransactionDocument)
        {
            existingTransaction.CancellationReasonCode = requestTransactionDocument.CancellationReasonCode;
            existingTransaction.StatusCode = Constants.Application.CancelTransactionStatus;
            existingTransaction.LastUpdatedBy = requestTransactionDocument.CancelledBy;
            existingTransaction.LastUpdatedOn = requestTransactionDocument.LastUpdatedOn;
            existingTransaction.CancelledBy = requestTransactionDocument.CancelledBy;

            TransactionCommandBase transactionData = new()
            {
                TransactionDocument = existingTransaction,
                CustomerId = requestTransactionDocument.CustomerId
            };

            var cloudEvent = new CloudEvent()
            {
                Data = transactionData,
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateTransaction.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateTransaction.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.TransactionDocSchemaVersion
            };

            await PublishEvent(cloudEvent);

            return (TransactionDTO)existingTransaction;
        }

        private async Task<TransactionDTO> CreateCompensatoryTransaction(TransactionDocument existingTransaction, TransactionDocument requestTransactionDocument, string prodName)
        {
            existingTransaction.CancelledTransactionId = requestTransactionDocument.TransactionId;
            existingTransaction.TransactionSubType = (existingTransaction.TransactionType == Constants.Application.AccrualTransactionType) ? Constants.Application.AccrualCancellation : Constants.Application.RedemptionCancellation;
            existingTransaction.StatusCode = Constants.Application.StatusCode;
            existingTransaction.TransactionDate = DateTime.UtcNow.ToString();
            existingTransaction.StatementText = Constants.Application.CancelTransactionStatus + " : " + existingTransaction.StatementText;
            existingTransaction.CreatedBy = requestTransactionDocument.CancelledBy;
            existingTransaction.LastUpdatedBy = requestTransactionDocument.CancelledBy;
            existingTransaction.CancellationReasonCode = string.Empty;
            existingTransaction.CancelledBy = string.Empty;

            if (existingTransaction.TransactionType == Constants.Application.AccrualTransactionType && existingTransaction.TransactionValue == "0" && existingTransaction.CreditDetails.Any())
            {
                existingTransaction.TransactionValue = existingTransaction.CreditDetails.Where(x => x.PointType == "Miles").Sum(s => Convert.ToInt32(s.CreditValue)).ToString();
            }

            if ((existingTransaction.TransactionType == Constants.Application.RedemptionTransactionType || prodName.Is(Constants.Application.VoucherPointsProductName)) && (requestTransactionDocument.CancellationReasonCode == "Within 24h of travel"|| requestTransactionDocument.CancellationReasonCode == "PPM - No refund of points"))
            {
                existingTransaction.TransactionValue = "0";
            }

            existingTransaction.CreditDetails = (existingTransaction.TransactionType == Constants.Application.AccrualTransactionType) ? null : existingTransaction.CreditDetails;
            existingTransaction.TransactionType = (existingTransaction.TransactionType == Constants.Application.AccrualTransactionType) ? Constants.Application.RedemptionTransactionType : Constants.Application.AccrualTransactionType;
            existingTransaction.CreatedOn = DateTime.UtcNow;
            existingTransaction.LastUpdatedOn = DateTime.UtcNow;
            existingTransaction.TransactionId = Utility.GetId();
            existingTransaction.TransactionChannel = requestTransactionDocument.TransactionChannel;
            existingTransaction.PointOverrideFlag = "N";

            TransactionCommandBase transactionData = new()
            {
                TransactionDocument = existingTransaction,
                CustomerId = requestTransactionDocument.CustomerId
            };

            var cloudEvent = new CloudEvent()
            {
                Data = transactionData,
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.CreateTransaction.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.CreateTransaction.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.TransactionDocSchemaVersion
            };

            await PublishEvent(cloudEvent);

            return (TransactionDTO)existingTransaction;
        }

        private async Task UpdateAward(TransactionDocument transaction, string awardDocId, string awardName)
        {
            Award award = new Award
            {
                Id = awardDocId,
                AwardId = transaction.AwardId,
                CustomerId = transaction.CustomerId,
                AwardName = awardName,
                Status = "Available",
                LastUpdatedBy = transaction.LastUpdatedBy,
            };

            AwardCommandBase awardData = new()
            {
                Award = award,
                CustomerId = transaction.CustomerId
            };

            var cloudEvent = new CloudEvent()
            {
                Data = awardData,
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateAward.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateAward.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.AwardDocSchemaVersion
            };

            await PublishEvent(cloudEvent);
        }

        private async Task PublishEvent(CloudEvent cloudEvent)
        {
            try
            {
                await _mediator.Send(new EventRequestInsertCommand
                {
                    Id = cloudEvent.Id,
                    Type = cloudEvent.Type,
                    Data = cloudEvent,
                    Subject = cloudEvent.Subject
                });

                await _mediator.Publish(cloudEvent);
            }
            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                throw;
            }
        }
    }
}
