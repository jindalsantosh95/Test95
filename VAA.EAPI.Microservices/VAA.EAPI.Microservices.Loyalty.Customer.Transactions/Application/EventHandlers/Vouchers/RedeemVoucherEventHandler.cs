using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Vouchers
{
    public class RedeemVoucherEventHandler : IRequestHandler<RedeemVoucherEvent, Result<Response<TransactionDTO>>>, IRedeemVoucherEventHandler
    {
        private readonly ILogger<RedeemVoucherEventHandler> _logger;
        private readonly IReferenceService _referenceService;
        private readonly AwardsQueryHandler _awardsQueryHandler;
        private readonly TransactionCreateEventHandler _transactionCreateEventHandler;
        private readonly FlyingClubMemberQueryHandler _flyingClubMemberQueryHandler;
        private readonly ITransactionStatementTextService _transactionStatementTextService;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;
        private readonly TransactionCommandStatusQueryHandler _transactionCommandStatusQueryHandler;
        private readonly ServiceResponseInsertCommandHandler _serviceResponseInsertCommandHandler;

        public RedeemVoucherEventHandler(ILogger<RedeemVoucherEventHandler> logger, IReferenceService referenceService, AwardsQueryHandler awardsQueryHandler, TransactionCreateEventHandler transactionCreateEventHandler, FlyingClubMemberQueryHandler flyingClubMemberQueryHandler, ITransactionStatementTextService transactionStatementTextService, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, TransactionCommandStatusQueryHandler transactionCommandStatusQueryHandler, ServiceResponseInsertCommandHandler serviceResponseInsertCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _referenceService = referenceService ?? throw new ArgumentNullException(nameof(referenceService));
            _awardsQueryHandler = awardsQueryHandler ?? throw new ArgumentNullException(nameof(awardsQueryHandler));
            _transactionCreateEventHandler = transactionCreateEventHandler ?? throw new ArgumentNullException(nameof(transactionCreateEventHandler));
            _flyingClubMemberQueryHandler = flyingClubMemberQueryHandler ?? throw new ArgumentNullException(nameof(flyingClubMemberQueryHandler));
            _transactionStatementTextService = transactionStatementTextService ?? throw new ArgumentNullException(nameof(transactionStatementTextService));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
            _transactionCommandStatusQueryHandler = transactionCommandStatusQueryHandler ?? throw new ArgumentNullException(nameof(transactionCommandStatusQueryHandler));
            _serviceResponseInsertCommandHandler = serviceResponseInsertCommandHandler ?? throw new ArgumentNullException(nameof(serviceResponseInsertCommandHandler));
        }

        public async Task<Result<Response<TransactionDTO>>> Handle(RedeemVoucherEvent request, CancellationToken cancellationToken)
        {
            Result<Response<TransactionDTO>> result = null;

            try
            {
                if (request?.Data is null)
                    return result = Result.Failure<Response<TransactionDTO>>("Redeem Voucher request payload is missing.");

                RedeemVoucherRequest createEvent = Json.DeserializeObject<RedeemVoucherRequest>((JToken)request.Data);

                var validationResult = new RedeemVoucherValidator().Validate(createEvent);

                if (!validationResult.IsValid)
                    throw new RequestValidationException("Redeem Voucher request contains invalid data or missing mandatory fields.", validationResult.Errors);

                Partner partnerDetails = (await _referenceService.GetAllPartnersAsync()).FirstOrDefault(x => x.PartnerCode == createEvent.PartnerCode || x.PartnerName == createEvent.PartnerName);
                IEnumerable<Product> productDetails = await _referenceService.GetAllProductsAsync();

                if (partnerDetails == null || productDetails == null || !productDetails.Any())
                    return result = Result.Failure<Response<TransactionDTO>>("Failed to retrieve Partner/Product Data");

                //Validate voucher status
                var awards = await _awardsQueryHandler.Handle(new AwardsQuery { CustomerId = createEvent.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.AwardDocType) }, cancellationToken);

                if (awards.IsFailure)
                    return result = Result.Failure<Response<TransactionDTO>>("Failed to retrieve awards");

                Award awardDetails = awards.Value.Value.Awards.FirstOrDefault(m => m.AwardNumber == createEvent.VoucherDetails.VoucherNumber);

                if (awardDetails is null)
                    return result = Result.Failure<Response<TransactionDTO>>("Voucher Number not found");

                if (awardDetails.Status != Constants.Application.Available && awardDetails.Status != Constants.Application.PartiallyUsed)
                    return result = Result.Failure<Response<TransactionDTO>>("Voucher Status is not applicable for redemption");

                //validate member status to create transaction
                var member = await _flyingClubMemberQueryHandler.Handle(new FlyingClubMemberQuery { CustomerId = createEvent.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.FlyingClubMemberDocType) }, cancellationToken);

                if (member.IsFailure)
                    return result = Result.Failure<Response<TransactionDTO>>("Failed to retrieve member details");

                Domain.FlyingClubMember.FlyingClubMember memberDetails = member.Value.Value.FlyingClubMemberDocuments.FirstOrDefault();

                if (memberDetails is null)
                    return result = Result.Failure<Response<TransactionDTO>>("Member details not found");

                if (!Constants.Application.MemberStatusesForAccrual.Contains(memberDetails.Status))
                    return result = Result.Failure<Response<TransactionDTO>>("Member status is invalid to create accrual transaction");

                bool isWingsMember = awards.Value.Value.Awards.Any(a =>
                {
                    if (string.IsNullOrWhiteSpace(a.ProductId))
                        return false;

                    var product = productDetails.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.ProductId) && p.ProductId.Trim().Is(a.ProductId.Trim()));

                    var productName = product?.ProductName;
                    if (string.IsNullOrWhiteSpace(productName) || !productName.Trim().Is(Constants.Application.WingsMemProductName))
                        return false;

                    return !a.ExpiryDate.HasValue || a.ExpiryDate.Value.Date >= DateTime.Now.Date;

                });

                //validate txn value based on tier
                int maxValue = VoucherValueHelper.GetTierBasedValue(memberDetails.TierCode, awardDetails.Status, isWingsMember);
                var vchrProdDetails = productDetails.FirstOrDefault(x => x.ProductId == awardDetails.ProductId);

                if (vchrProdDetails is null)
                    return result = Result.Failure<Response<TransactionDTO>>("Voucher product not found");

                if (createEvent.VoucherDetails.PointsToAward.Value > maxValue)
                    return result = Result.Failure<Response<TransactionDTO>>("Points to award value cannot be greater than member tier based point value for a voucher");

                //create voucher accrual transaction
                //Get statement text
                var accrlProdDetails = productDetails.FirstOrDefault(x => x.ProductName == Constants.Application.VoucherPointsProductName);

                if (accrlProdDetails is null)
                    return result = Result.Failure<Response<TransactionDTO>>("Accrual product not found");

                TransactionDocument transactionData = (TransactionDocument)(createEvent, partnerDetails.PartnerId, accrlProdDetails.ProductId, awardDetails.AwardId, Constants.Application.AccrualTransactionType);
                transactionData.StatementText = await _transactionStatementTextService.GetTransactionStatementText((StatementTextRequest)(transactionData, partnerDetails, accrlProdDetails, null), cancellationToken);

                var createTxnResponse = await InvokeCreateTransactionHandler(transactionData, cancellationToken);

                if (createTxnResponse == Status.OK)
                {
                    //create accrual txn in vred
                    CloudEvent vredCreateAccrlEvent = TransactionMapper.MapVredCreateTxnReqToCloudEvent(new VredCreateTransactionEventCommand { Request = (VredCreateTransactionRequest)(transactionData, partnerDetails, accrlProdDetails, vchrProdDetails, awardDetails, Constants.Application.Used) }, request.Id);

                    AsyncContext.Current.MapToCloudEvent(vredCreateAccrlEvent);

                    await PublishVredEvent(vredCreateAccrlEvent, cancellationToken);

                    //create redemption txn in cosmos
                    if (!memberDetails.Status.Is(Constants.Application.ActiveStatus))
                        return result = Result.Failure<Response<TransactionDTO>>("Member status is invalid to create redemption transaction");

                    var rdmProdDetails = productDetails.FirstOrDefault(x => x.ProductName == createEvent.RedemptionType);

                    if (rdmProdDetails is null)
                        return result = Result.Failure<Response<TransactionDTO>>("Redemption product not found");

                    TransactionDocument rdmTxnData = (TransactionDocument)(createEvent, partnerDetails.PartnerId, rdmProdDetails.ProductId, awardDetails.AwardId, Constants.Application.RedemptionTransactionType);
                    rdmTxnData.StatementText = await _transactionStatementTextService.GetTransactionStatementText((StatementTextRequest)(rdmTxnData, partnerDetails, rdmProdDetails, vchrProdDetails.ProductName.Contains('-') ? vchrProdDetails.ProductName.Split('-').ElementAt(1) : null), cancellationToken);

                    var createRdmTxnResp = await InvokeCreateTransactionHandler(rdmTxnData, cancellationToken);

                    if (createRdmTxnResp == Status.OK)
                    {
                        //create redemption txn in vred
                        CloudEvent vredCreateRdmEvent = TransactionMapper.MapVredCreateTxnReqToCloudEvent(new VredCreateTransactionEventCommand { Request = (VredCreateTransactionRequest)(rdmTxnData, partnerDetails, rdmProdDetails, vchrProdDetails, awardDetails, Constants.Application.Used) }, Guid.NewGuid().ToString());

                        AsyncContext.Current.MapToCloudEvent(vredCreateRdmEvent);

                        await PublishVredEvent(vredCreateRdmEvent, cancellationToken);

                        //update voucher Status
                        await UpdateAwardDoc(rdmTxnData, awardDetails.Id, vchrProdDetails.ProductName, cancellationToken);

                        return result = Result.Success(
                                new Response<TransactionDTO>()
                                {
                                    Status = Status.OK,
                                    Value = (TransactionDTO)transactionData
                                });
                    }
                    else
                    {
                        return result = Result.Failure<Response<TransactionDTO>>("Create redemption transaction event failure");
                    }
                }
                else
                {
                    return result = Result.Failure<Response<TransactionDTO>>("Create accrual transaction event failure");
                }
            }
            catch (RequestValidationException rvex)
            {
                return result = Result.Success(
                    new Response<TransactionDTO>()
                    {
                        Status = Status.BusinessException,
                        Errors = (await TransactionBaseEventHandler.MapValidationErrors(rvex)).ToList()
                    }
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred in RedeemVoucherEventHandler: {ex.Message}");
                return result = Result.Failure<Response<TransactionDTO>>(ex.Message.ToString());
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

        private async Task<Status> InvokeCreateTransactionHandler(TransactionDocument txnDoc, CancellationToken cancellationToken)
        {
            TransactionCommandBase transactionData = new()
            {
                TransactionDocument = txnDoc,
                CustomerId = txnDoc.CustomerId
            };

            var createTxnEvent = TransactionMapper.MapTxnCreateRequestToCloudEvent(transactionData);

            AsyncContext.Current.MapToCloudEvent(createTxnEvent);

            await _transactionCreateEventHandler.Handle(createTxnEvent, cancellationToken);

            var txnStatus = await _transactionCommandStatusQueryHandler.Handle(new TransactionCommandStatusQuery { RequestId = createTxnEvent.Id }, cancellationToken);

            if (txnStatus.IsSuccess && txnStatus.Value.Status == Status.OK && txnStatus.Value.Value.Transaction != null)
            {
                return Status.OK;
            }
            else
            {
                return Status.Error;
            }

        }

        private async Task PublishCosmosEvent(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

            try
            {
                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName) }, cancellationToken);
            }
            catch
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                throw;
            }
        }

        private async Task UpdateAwardDoc(TransactionDocument transaction, string awardDocId, string awardName, CancellationToken cancellationToken)
        {
            Award award = new Award
            {
                Id = awardDocId,
                AwardId = transaction.AwardId,
                CustomerId = transaction.CustomerId,
                Status = Constants.Application.Used,
                UsedDate = DateTime.UtcNow,
                LastUpdatedBy = transaction.LastUpdatedBy,
                AwardName = awardName
            };

            AwardCommandBase awardData = new()
            {
                Award = award,
                CustomerId = transaction.CustomerId
            };

            var cloudEvent = new CloudEvent()
            {
                Id = Guid.NewGuid().ToString(),
                Data = awardData,
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateAward.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateAward.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.AwardDocSchemaVersion
            };

            await PublishCosmosEvent(cloudEvent, cancellationToken);
        }
    }
}
