using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Events
{
    public class UpdateTransactionPnrEventHandler : IUpdateTransactionPnrEventHandler
    {
        private readonly IUpdateVredTransactionServiceHandler _updateVredTransactionServiceQueryHandler;
        private readonly VredStatementsRepoHandler _vredStatementsRepoHandler;
        private readonly ILogger<UpdateTransactionPnrEventHandler> _logger;
        private readonly ServiceResponseInsertCommandHandler _serviceResponseInsertCommandHandler;
        private readonly ITransactionDBRepository _transactionDBRepository;

        public UpdateTransactionPnrEventHandler(IUpdateVredTransactionServiceHandler updateVredTransactionServiceQueryHandler, VredStatementsRepoHandler vredStatementsRepoHandler, ILogger<UpdateTransactionPnrEventHandler> logger, ServiceResponseInsertCommandHandler serviceResponseInsertCommandHandler, ITransactionDBRepository transactionDBRepository)
        {
            _updateVredTransactionServiceQueryHandler = updateVredTransactionServiceQueryHandler ?? throw new ArgumentNullException(nameof(updateVredTransactionServiceQueryHandler));
            _vredStatementsRepoHandler = vredStatementsRepoHandler ?? throw new ArgumentNullException(nameof(vredStatementsRepoHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceResponseInsertCommandHandler = serviceResponseInsertCommandHandler ?? throw new ArgumentNullException(nameof(serviceResponseInsertCommandHandler));
            _transactionDBRepository = transactionDBRepository ?? throw new ArgumentNullException(nameof(transactionDBRepository));
        }

        public async Task<Result<Response<string>>> Handle(UpdateTransactionPnrEvent request, CancellationToken cancellationToken)
        {
            Result<Response<string>> result = null;
            var response = new List<UpdateTransactionResponse>();

            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                UpdateTransactionPnrRequest eventData = Json.Deserialize<UpdateTransactionPnrRequest>(Convert.ToString(request.Data));

                if (string.IsNullOrEmpty(eventData.MemberNumber)) throw new ArgumentNullException(nameof(eventData.MemberNumber));
                if (string.IsNullOrEmpty(eventData.UpdatedBy)) throw new ArgumentNullException(nameof(eventData.UpdatedBy));
                if (eventData.TransactionDetails == null) throw new ArgumentNullException(nameof(eventData.TransactionDetails));

                //Retrieve Transaction
                var memberTransactions = await _vredStatementsRepoHandler.Handle(new VredStatementsRepoQuery(eventData.MemberNumber), cancellationToken);

                if (memberTransactions is null)
                    throw new ValidationException($"No Vred Transaction found for member {eventData.MemberNumber}");

                //attribute to track update status
                var failedResp = new List<string>();

                //initiate update txn
                foreach (var updateTxnReq in eventData.TransactionDetails)
                {
                    CreditMetaData earnTransactionDetails = null;
                    DebitMetaData burnTransactionDetails = null;

                    if (string.IsNullOrEmpty(updateTxnReq.TransactionId) || string.IsNullOrEmpty(updateTxnReq.BookingReference) || string.IsNullOrEmpty(updateTxnReq.TransactionType))
                    {
                        failedResp.Add("Missing Mandatory attribute");
                        continue;
                    }

                    if (updateTxnReq.TransactionType.Is(Constants.Application.AccrualType))
                    {
                        var earnTransaction = memberTransactions.Statements.Where(t => t.Type == Constants.Application.CreditTransaction)
                        .Select(t => JsonConvert.DeserializeObject<CreditMetaDataOut>(t.Details))
                        .FirstOrDefault(t => t is not null && t.TransactionId == updateTxnReq.TransactionId);

                        earnTransactionDetails = JsonConvert.DeserializeObject<CreditMetaData>(JsonConvert.SerializeObject(earnTransaction));
                    }
                    else
                    {
                        var burnTransaction = memberTransactions.Statements.Where(t => t.Type == Constants.Application.DebitTransaction)
                            .Select(t => JsonConvert.DeserializeObject<DebitMetaDataOut>(t.Details))
                            .FirstOrDefault(t => t is not null && t.TransactionId == updateTxnReq.TransactionId);

                        burnTransactionDetails = JsonConvert.DeserializeObject<DebitMetaData>(JsonConvert.SerializeObject(burnTransaction));
                    }

                    if (earnTransactionDetails is null && burnTransactionDetails is null)
                    {
                        failedResp.Add($"Vred Transaction not found for: {updateTxnReq.TransactionId}");
                        continue;
                    }

                    //insert update request to transaction db
                    var auditInfo = (AuditTransactionInfo)(eventData, updateTxnReq, earnTransactionDetails, burnTransactionDetails);

                    await _transactionDBRepository.AddTransactionAuditInfo(auditInfo.MembershipNumber, auditInfo.TransactionID, auditInfo.Status, auditInfo.TransactionType, auditInfo.ActivityDate, auditInfo.TransactionProcessedDate, auditInfo.TransactionSource, auditInfo.ServiceRequest,
                                                                       auditInfo.SiebelRequest, auditInfo.SiebelResponse, auditInfo.CreatedBy, auditInfo.CreatedByEmailId, auditInfo.CreatedByUserName);

                    var updateVredRequest = (UpdateVredTransactionRequest)(eventData, updateTxnReq, earnTransactionDetails, burnTransactionDetails);

                    //update vred request to transaction db
                    await _transactionDBRepository.UpdateVRedRequestAuditInfo(updateTxnReq.TransactionId, Json.Serialize(updateVredRequest));

                    //invoke update txn
                    var updateResponse = await _updateVredTransactionServiceQueryHandler.Handle(new UpdateVredTransactionService(updateVredRequest), cancellationToken);

                    //update vred response to transaction db
                    if (updateResponse != null)
                        await _transactionDBRepository.UpdateVRedResponseAuditInfo(updateTxnReq.TransactionId, Json.Serialize(updateResponse));

                    if (updateResponse is null || updateResponse.ErrorResponse != null)
                    {
                        failedResp.Add($"UpdateVredTransaction response doesn't indicate success for: {updateTxnReq.TransactionId}");
                        continue;
                    }

                    response.Add(updateResponse);
                }

                if (failedResp.Any())
                {
                    return result = Result.Failure<Response<string>>($"One or more failures - {string.Join(", ", failedResp)}");
                }
                else
                {
                    return result = new Response<string>
                    {
                        Status = Status.OK,
                        Value = Json.Serialize(response)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateTransactionPnrEventHandler failed due to Exception");
                return result = Result.Failure<Response<string>>(ex.Message.ToString());
            }
            finally
            {
                //insert result to event store
                if (request != null)
                    await _serviceResponseInsertCommandHandler.Handle(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = result.IsFailure ? ServiceResponseProcessStatus.Error : ServiceResponseProcessStatus.Completed, Type = request.Type }, cancellationToken);
            }
        }

    }
}
