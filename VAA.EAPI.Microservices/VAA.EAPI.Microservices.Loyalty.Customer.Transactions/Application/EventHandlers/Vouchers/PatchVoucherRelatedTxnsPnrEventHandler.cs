using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Vouchers
{
    public class PatchVoucherRelatedTxnsPnrEventHandler : IPatchVoucherRelatedTxnsPnrEventHandler
    {
        private readonly ILogger<PatchVoucherRelatedTxnsPnrEventHandler> _logger;
        private readonly IVoucherRelatedTransactionQueryHandler _voucherRelatedTxnsQueryHandler;
        private readonly ServiceResponseInsertCommandHandler _serviceResponseInsertCommandHandler;
        private readonly TransactionCommandStatusQueryHandler _transactionCommandStatusQueryHandler;
        private readonly TransactionUpdateEventHandler _transactionUpdateEventHandler;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;

        public PatchVoucherRelatedTxnsPnrEventHandler(ILogger<PatchVoucherRelatedTxnsPnrEventHandler> logger, IVoucherRelatedTransactionQueryHandler voucherRelatedTxnsQueryHandler, ServiceResponseInsertCommandHandler serviceResponseInsertCommandHandler, TransactionCommandStatusQueryHandler transactionCommandStatusQueryHandler, TransactionUpdateEventHandler transactionUpdateEventHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _voucherRelatedTxnsQueryHandler = voucherRelatedTxnsQueryHandler ?? throw new ArgumentNullException(nameof(voucherRelatedTxnsQueryHandler));
            _serviceResponseInsertCommandHandler = serviceResponseInsertCommandHandler ?? throw new ArgumentNullException(nameof(serviceResponseInsertCommandHandler));
            _transactionCommandStatusQueryHandler = transactionCommandStatusQueryHandler ?? throw new ArgumentNullException(nameof(transactionCommandStatusQueryHandler));
            _transactionUpdateEventHandler = transactionUpdateEventHandler ?? throw new ArgumentNullException(nameof(transactionUpdateEventHandler));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        }

        public async Task<Result<Response<IEnumerable<TransactionDTO>>>> Handle(PatchVoucherRelatedTxnsPnrEvent request, CancellationToken cancellationToken)
        {
            Result<Response<IEnumerable<TransactionDTO>>> result = null;
            var patchResponse = new ConcurrentBag<TransactionDTO>();

            try
            {
                if (request?.Data is null)
                    return result = Result.Failure<Response<IEnumerable<TransactionDTO>>>("Patch Voucher related txns request payload is missing.");

                PatchVoucherRelatedTxnsPnrRequest patchEvent = Json.DeserializeObject<PatchVoucherRelatedTxnsPnrRequest>((JToken)request.Data);

                var validationResult = new PatchVoucherRelatedTxnsPnrValidator().Validate(patchEvent);

                if (!validationResult.IsValid)
                    throw new RequestValidationException("Patch Voucher related txns request contains invalid data or missing mandatory fields.", validationResult.Errors);

                //attribute to track update status
                var failedVouchers = new ConcurrentBag<string>();
                var vredRequest = new ConcurrentBag<VredUpdateTransactionPnrDetails>();

                //initiate update of txns
                await Parallel.ForEachAsync(patchEvent.VoucherNumbers, async (voucher, cancellationToken) =>
                {
                    //get all related txns for the voucher
                    var vchrRelatedTxnResp = await _voucherRelatedTxnsQueryHandler.GetVoucherRelatedTransactions(new VoucherRelatedTransactionQuery { CustomerId = patchEvent.CustomerId, VoucherNumber = voucher });

                    if (vchrRelatedTxnResp.IsFailure || vchrRelatedTxnResp.Value.Value is null || !vchrRelatedTxnResp.Value.Value.TransactionDocuments.Any())
                    {
                        failedVouchers.Add($"Retrieve VoucherTxns: {voucher}");
                    }
                    else
                    {

                        foreach (var txn in vchrRelatedTxnResp.Value.Value.TransactionDocuments)
                        {
                            //update txn details
                            txn.FlightDetails ??= new Flightdetails();
                            txn.FlightDetails.ExternalReservationNumber = patchEvent.PNR;
                            txn.LastUpdatedBy = patchEvent.InitiatedBy;
                            txn.LastUpdatedOn = DateTime.UtcNow;

                            //invoke txn update handler 
                            var updateResponse = await InvokeUpdateTransactionHandler(txn, cancellationToken);

                            if (updateResponse == Status.OK)
                            {
                                patchResponse.Add(new TransactionDTO { TransactionId = txn.TransactionId, TransactionType = txn.TransactionType, AwardId = txn.AwardId });

                                //add txns for vred update pnr event
                                vredRequest.Add(new VredUpdateTransactionPnrDetails { TransactionId = txn.TransactionId, BookingReference = patchEvent.PNR, TransactionType = txn.TransactionType });
                            }
                            else
                            {
                                failedVouchers.Add($"Update Transaction: {txn.TransactionId} with voucher: {voucher}");
                            }
                        }
                    }
                });

                //publish event to update pnr in vred
                if (vredRequest.Any())
                {
                    CloudEvent vredUpdatePnrEvent = TransactionMapper.MapVredUpdateTxnPnrReqToCloudEvent((VredUpdateTransactionPnrRequest)(patchEvent, vredRequest), request.Id);

                    AsyncContext.Current.MapToCloudEvent(vredUpdatePnrEvent);

                    await PublishVredEvent(vredUpdatePnrEvent, cancellationToken);
                }

                if (failedVouchers.Any())
                {
                    return result = Result.Failure<Response<IEnumerable<TransactionDTO>>>($"One or more process failures - {string.Join(",", failedVouchers)}");
                }
                else
                {
                    return result = Result.Success(
                        new Response<IEnumerable<TransactionDTO>>()
                        {
                            Status = Status.OK,
                            Value = patchResponse
                        }
                        );
                }
            }
            catch (RequestValidationException rvex)
            {
                return result = Result.Success(
                    new Response<IEnumerable<TransactionDTO>>()
                    {
                        Status = Status.BusinessException,
                        Errors = (await TransactionBaseEventHandler.MapValidationErrors(rvex)).ToList()
                    }
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred in PatchVoucherRelatedTxnsEventHandler: {ex.Message}");
                return result = Result.Failure<Response<IEnumerable<TransactionDTO>>>(ex.Message.ToString());
            }
            finally
            {
                //insert result to event store
                if (request != null)
                    await _serviceResponseInsertCommandHandler.Handle(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = result.IsFailure ? ServiceResponseProcessStatus.Error : ServiceResponseProcessStatus.Completed, Type = Constants.Application.VoucherRedemptionCommandResponseStatusKey }, cancellationToken);
            }
        }

        private async Task<Status> InvokeUpdateTransactionHandler(TransactionDocument txnDoc, CancellationToken cancellationToken)
        {
            TransactionCommandBase transactionData = new()
            {
                TransactionDocument = txnDoc,
                CustomerId = txnDoc.CustomerId
            };

            var patchTxnEvent = TransactionMapper.MapTxnUpdateRequestToCloudEvent(transactionData);

            AsyncContext.Current.MapToCloudEvent(patchTxnEvent);

            await _transactionUpdateEventHandler.Handle(patchTxnEvent, cancellationToken);

            var txnStatus = await _transactionCommandStatusQueryHandler.Handle(new TransactionCommandStatusQuery { RequestId = patchTxnEvent.Id }, cancellationToken);

            if (txnStatus.IsSuccess && txnStatus.Value.Status == Status.OK && txnStatus.Value.Value.Transaction != null)
            {
                return Status.OK;
            }
            else
            {
                return Status.Error;
            }

        }

        private async Task PublishVredEvent(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

            try
            {
                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.EnvironmentSettings.VredServiceBusTopicName) }, cancellationToken);
            }
            catch
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                throw;
            }
        }
    }
}
