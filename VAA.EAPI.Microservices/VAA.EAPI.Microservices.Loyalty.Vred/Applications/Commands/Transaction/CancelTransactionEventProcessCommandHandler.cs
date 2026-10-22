using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Enum;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction
{
    public class CancelTransactionEventProcessCommandHandler : IRequestHandler<CancelTransactionEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ITransactionDBRepository _transactionDBRepository;
        private readonly ILogger<CancelTransactionEventProcessCommandHandler> _logger;


        public CancelTransactionEventProcessCommandHandler(IMediator mediator, ILogger<CancelTransactionEventProcessCommandHandler> logger, ITransactionDBRepository transactionDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _transactionDBRepository = transactionDBRepository ?? throw new ArgumentNullException(nameof(transactionDBRepository));
        }

        public async Task<Result<Response<string>>> Handle(CancelTransactionEventProcessCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                CancelTransactionRequest cancelTransactionRequest = Json.Deserialize<CancelTransactionRequest>(Convert.ToString(request.Data));
                CreditMetaData earnTransactionDetails = new();
                DebitMetaData burnTransactionDetails = new();

                var auditInfo = new AuditTransactionInfo
                {
                    ServiceRequest = Json.Serialize(request.Data),
                    MembershipNumber = cancelTransactionRequest.MemberNumber,
                    TransactionSource = cancelTransactionRequest.TransactionChannel,
                    CreatedBy = cancelTransactionRequest.UpdatedBy.IsNullOrEmpty() ? Constants.Application.CreatedUser : cancelTransactionRequest.UpdatedBy,
                    CreatedByEmailId = cancelTransactionRequest.UpdatedBy.IsNullOrEmpty() ? Constants.Application.CreatedUser : cancelTransactionRequest.UpdatedBy,
                    CreatedByUserName = cancelTransactionRequest.UpdatedBy.IsNullOrEmpty() ? Constants.Application.CreatedUser : cancelTransactionRequest.UpdatedBy,
                    TransactionID = cancelTransactionRequest.TransactionId,
                    Status = Constants.Application.VRedActivityStatusCancelled,
                    ActivityDate = cancelTransactionRequest.ActivityDate.ToString(Constants.Application.VRedRequestDateFormat),
                    TransactionProcessedDate = DateTime.UtcNow.ToString(Constants.Application.VRedRequestDateFormat)
                };

                //Retrieve Transaction
                var memberTransactions = await _mediator.Send(new VredStatementsRepoQuery(cancelTransactionRequest.MemberNumber));

                var earnTransaction = memberTransactions.Statements.Where(t => t.Type == Constants.Application.CreditTransaction)
                    .Select(t => JsonConvert.DeserializeObject<CreditMetaDataOut>(t.Details))
                    .FirstOrDefault(t => t is not null && t.TransactionId == cancelTransactionRequest.TransactionId && t.Status != Constants.Application.CancelStatus);

                var burnTransaction = memberTransactions.Statements.Where(t => t.Type == Constants.Application.DebitTransaction)
                    .Select(t => JsonConvert.DeserializeObject<DebitMetaDataOut>(t.Details))
                    .FirstOrDefault(t => t is not null && t.TransactionId == cancelTransactionRequest.TransactionId && t.Status != Constants.Application.CancelStatus);

                if (earnTransaction != null)
                {
                    earnTransactionDetails = JsonConvert.DeserializeObject<CreditMetaData>(JsonConvert.SerializeObject(earnTransaction));
                }
                else
                {
                    burnTransactionDetails = JsonConvert.DeserializeObject<DebitMetaData>(JsonConvert.SerializeObject(burnTransaction));
                }

                if (!string.IsNullOrEmpty(earnTransactionDetails.ActivityType))
                {
                    if (earnTransactionDetails.ActivityType.Equals(Constants.Application.AccrualType))
                    {
                        auditInfo.TransactionType = Constants.Application.EarnActivityType;

                    }
                }
                else
                {
                    if (burnTransactionDetails.ActivityType.Equals(Constants.Application.RedemptionType))
                    {
                        auditInfo.TransactionType = Constants.Application.BurnActivityType;
                    }
                }

                await _transactionDBRepository.AddTransactionAuditInfo(auditInfo.MembershipNumber, auditInfo.TransactionID, auditInfo.Status, auditInfo.TransactionType, auditInfo.ActivityDate, auditInfo.TransactionProcessedDate, auditInfo.TransactionSource, auditInfo.ServiceRequest,
                                                                        auditInfo.SiebelRequest, auditInfo.SiebelResponse, auditInfo.CreatedBy, auditInfo.CreatedByEmailId, auditInfo.CreatedByUserName);

                var cancelVredTransactionData = CancelVredTransactionDataMapper.CancelVredTransactionRequestMapper(cancelTransactionRequest, earnTransactionDetails, burnTransactionDetails);

                var VRedRequestJson = Json.Serialize(cancelVredTransactionData);

                await _transactionDBRepository.UpdateVRedRequestAuditInfo(auditInfo.TransactionID, VRedRequestJson);

                //Update status of current transaction
                UpdateTransactionResponse cancelTransactionResponse = await _mediator.Send(new CancelVredTransactionRepoQuery(cancelVredTransactionData), cancellationToken);

                //Create Compensatory Transaction Logic wil be here
                if (cancelTransactionResponse.SuccessResponse != null)
                {
                    if (!string.IsNullOrEmpty(earnTransactionDetails.ActivityType))
                    {
                        await _mediator.Send(CancelVredTransactionDataMapper.CompensatoryBurnTransactionRequestMapper(cancelTransactionRequest, earnTransactionDetails, request.Id));
                    }
                    else
                    {
                        await _mediator.Send(CancelVredTransactionDataMapper.CompensatoryEarnTransactionRequestMapper(cancelTransactionRequest, burnTransactionDetails, request.Id));
                    }


                    var result = new Response<string>
                    {
                        Status = Status.OK,
                        Value = Json.Serialize(cancelTransactionResponse.SuccessResponse)
                    };

                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.CancelTransactionEvent.ToString())) });

                    var VRedResponseJson = System.Text.Json.JsonSerializer.Serialize(cancelTransactionResponse.SuccessResponse);

                    await _transactionDBRepository.UpdateVRedResponseAuditInfo(auditInfo.TransactionID, VRedResponseJson);

                    return result;
                }
                else
                {
                    var result = new Response<string>
                    {
                        Status = Status.BusinessException,
                        Value = Json.Serialize(cancelTransactionResponse.ErrorResponse)
                    };

                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.CancelTransactionEvent.ToString())) });

                    var VRedResponseJson = System.Text.Json.JsonSerializer.Serialize(cancelTransactionResponse.ErrorResponse);

                    await _transactionDBRepository.UpdateVRedResponseAuditInfo(auditInfo.TransactionID, VRedResponseJson);

                    return result;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CancelTransactionEventProcessCommandHandler failed due to Exception");

                var exception = ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(exception), Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.CancelTransactionEvent.ToString())) });

                return CSharpFunctionalExtensions.Result.Failure<Response<string>>(Json.Serialize(exception));
            }
        }
    }
}
