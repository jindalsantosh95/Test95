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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Vouchers
{
    public class RollbackVoucherRdmEventHandler : IRollbackVoucherRdmEventHandler
    {
        private readonly ILogger<RollbackVoucherRdmEventHandler> _logger;
        private readonly AwardsQueryHandler _awardsQueryHandler;
        private readonly IVoucherRelatedTransactionQueryHandler _voucherRelatedTxnsQueryHandler;
        private readonly IReferenceService _referenceService;
        private readonly TransactionCancelEventHandler _transactionCancelEventHandler;
        private readonly ServiceResponseInsertCommandHandler _serviceResponseInsertCommandHandler;
        private readonly TransactionCommandStatusQueryHandler _transactionCommandStatusQueryHandler;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;

        public RollbackVoucherRdmEventHandler(ILogger<RollbackVoucherRdmEventHandler> logger, AwardsQueryHandler awardsQueryHandler, IVoucherRelatedTransactionQueryHandler voucherRelatedTxnsQueryHandler, IReferenceService referenceService, TransactionCancelEventHandler transactionCancelEventHandler, ServiceResponseInsertCommandHandler serviceResponseInsertCommandHandler, TransactionCommandStatusQueryHandler transactionCommandStatusQueryHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _awardsQueryHandler = awardsQueryHandler ?? throw new ArgumentNullException(nameof(awardsQueryHandler));
            _voucherRelatedTxnsQueryHandler = voucherRelatedTxnsQueryHandler ?? throw new ArgumentNullException(nameof(voucherRelatedTxnsQueryHandler));
            _referenceService = referenceService ?? throw new ArgumentNullException(nameof(referenceService));
            _transactionCancelEventHandler = transactionCancelEventHandler ?? throw new ArgumentNullException(nameof(transactionCancelEventHandler));
            _serviceResponseInsertCommandHandler = serviceResponseInsertCommandHandler ?? throw new ArgumentNullException(nameof(serviceResponseInsertCommandHandler));
            _transactionCommandStatusQueryHandler = transactionCommandStatusQueryHandler ?? throw new ArgumentNullException(nameof(transactionCommandStatusQueryHandler));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        }

        public async Task<Result<Response<IEnumerable<TransactionDTO>>>> Handle(RollbackVoucherRdmEvent request, CancellationToken cancellationToken)
        {
            Result<Response<IEnumerable<TransactionDTO>>> result = null;
            var cancelResponse = new ConcurrentBag<TransactionDTO>();

            try
            {
                if (request?.Data is null)
                    return result = Result.Failure<Response<IEnumerable<TransactionDTO>>>("Rollback Voucher redemption request payload is missing.");

                RollbackVoucherRdmRequest cancelEvent = Json.DeserializeObject<RollbackVoucherRdmRequest>((JToken)request.Data);

                var validationResult = new RollbackVoucherRdmValidator().Validate(cancelEvent);

                if (!validationResult.IsValid)
                    throw new RequestValidationException("Rollback Voucher redemption request contains invalid data or missing mandatory fields.", validationResult.Errors);

                //Validate voucher and fetch award id
                var awards = await _awardsQueryHandler.Handle(new AwardsQuery { CustomerId = cancelEvent.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.AwardDocType) }, cancellationToken);

                if (awards.IsFailure)
                    return result = Result.Failure<Response<IEnumerable<TransactionDTO>>>("Failed to retrieve awards");

                Award awardDetails = awards.Value.Value.Awards.FirstOrDefault(m => m.AwardNumber == cancelEvent.VoucherNumber);

                if (awardDetails is null)
                    return result = Result.Failure<Response<IEnumerable<TransactionDTO>>>("Voucher Number not found");

                //fetch related transactions based on award id
                var vchrRelatedTxnResp = await _voucherRelatedTxnsQueryHandler.GetVoucherRelatedTransactions(new VoucherRelatedTransactionQuery { AwardId = awardDetails.AwardId, CustomerId = cancelEvent.CustomerId, VoucherNumber = awardDetails.AwardNumber });

                if (vchrRelatedTxnResp.IsFailure)
                    return result = Result.Failure<Response<IEnumerable<TransactionDTO>>>("Failed to retrieve related transaction data");

                TransactionResponse vchrRelatedTxns = vchrRelatedTxnResp.Value.Value;

                if (vchrRelatedTxns is null || !vchrRelatedTxns.TransactionDocuments.Any())
                    return result = Result.Failure<Response<IEnumerable<TransactionDTO>>>("Voucher related transaction not found");

                //Fetch referencedata
                IEnumerable<Partner> partnerDetails = await _referenceService.GetAllPartnersAsync();
                IEnumerable<Product> productDetails = await _referenceService.GetAllProductsAsync();

                if (partnerDetails == null || !partnerDetails.Any() || productDetails == null || !productDetails.Any())
                    return result = Result.Failure<Response<IEnumerable<TransactionDTO>>>("Failed to retrieve Partner/Product Data");

                //attribute to track rollback status
                int rollbackRequestCount = vchrRelatedTxns.TransactionDocuments.Count();
                int cancelledTxnCount = rollbackRequestCount;
                var failedTxnIds = new ConcurrentBag<string>();

                //initiate rollback of txns
                await Parallel.ForEachAsync(vchrRelatedTxns.TransactionDocuments, async (txnData, cancellationToken) =>
                {
                    try
                    {
                        TransactionCommandBase txnCancelRequest = new()
                        {
                            TransactionDocument = (TransactionDocument)(cancelEvent, txnData, productDetails.FirstOrDefault(x => x.ProductId == awardDetails.ProductId)?.ProductName),
                            CustomerId = cancelEvent.CustomerId,
                            PartnerName = partnerDetails.FirstOrDefault(x => x.PartnerId == txnData.PartnerId)?.PartnerName,
                            ProductName = productDetails.FirstOrDefault(x => x.ProductId == txnData.ProductId)?.ProductName
                        };

                        TransactionCancelEvent cancelCloudEvent = TransactionMapper.MapTxnCancelRequestToCloudEvent(txnCancelRequest);

                        AsyncContext.Current.MapToCloudEvent(cancelCloudEvent);

                        await _transactionCancelEventHandler.Handle(cancelCloudEvent, cancellationToken);

                        var cancelTxnStatus = await _transactionCommandStatusQueryHandler.Handle(new TransactionCommandStatusQuery { RequestId = cancelCloudEvent.Id }, cancellationToken);

                        string compensatoryTxnId = cancelTxnStatus.IsSuccess && cancelTxnStatus.Value.Status == Status.OK ? cancelTxnStatus.Value.Value?.Transaction?.TransactionId : null;

                        if (compensatoryTxnId != null)
                        {
                            cancelResponse.Add((TransactionDTO)cancelTxnStatus.Value.Value.Transaction);

                            //publish event for cancellation in vred
                            CloudEvent vredCancelRdmEvent = TransactionMapper.MapVredCancelTxnReqToCloudEvent((VredCancelTransactionRequest)(txnCancelRequest.TransactionDocument, compensatoryTxnId), txnCancelRequest.ProductName == Constants.Application.VoucherPointsProductName ? request.Id : Guid.NewGuid().ToString());

                            AsyncContext.Current.MapToCloudEvent(vredCancelRdmEvent);

                            await PublishVredEvent(vredCancelRdmEvent, cancellationToken);
                        }
                        else
                        {
                            cancelledTxnCount--;
                            failedTxnIds.Add(txnData.TransactionId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Exception occurred while invoking transaction cancel event handler for transaction id {txnData.TransactionId} with message: {ex.Message}");
                        cancelledTxnCount--;
                        failedTxnIds.Add(txnData.TransactionId);
                    }
                });

                if (cancelledTxnCount == rollbackRequestCount)
                {
                    return result = Result.Success(
                               new Response<IEnumerable<TransactionDTO>>()
                               {
                                   Status = Status.OK,
                                   Value = cancelResponse
                               });
                }
                else
                {
                    return result = Result.Failure<Response<IEnumerable<TransactionDTO>>>($"One or more voucher transaction rollback failed - {string.Join("','", failedTxnIds)}");
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
                _logger.LogError(ex, $"Exception occurred in RollbackVoucherRdmEventHandler: {ex.Message}");
                return result = Result.Failure<Response<IEnumerable<TransactionDTO>>>(ex.Message.ToString());
            }
            finally
            {
                //insert result to event store
                if (request != null)
                    await _serviceResponseInsertCommandHandler.Handle(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = result.IsFailure ? ServiceResponseProcessStatus.Error : ServiceResponseProcessStatus.Completed, Type = Constants.Application.VoucherRedemptionCommandResponseStatusKey }, cancellationToken);
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
