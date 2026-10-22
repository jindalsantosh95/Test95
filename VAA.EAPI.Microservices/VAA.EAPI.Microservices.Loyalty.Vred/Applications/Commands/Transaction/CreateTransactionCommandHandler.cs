using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Enum;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction
{
    public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, VRedTransactionResponse>
    {
        protected readonly IMediator _mediator;
        private readonly ITransactionDBRepository _transactionDBRepository;

        public CreateTransactionCommandHandler(IMediator mediator, ITransactionDBRepository transactionDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _transactionDBRepository = transactionDBRepository ?? throw new ArgumentNullException(nameof(transactionDBRepository));
        }

        public async Task<VRedTransactionResponse> Handle(CreateTransactionCommand command, CancellationToken cancellationToken)
        {
            VRedTransactionResponse transactionResponse = new VRedTransactionResponse()
            {
                SuccessResponse = null,
                ErrorResponse = new VRedErrorResponse()
                {
                    Code = "500",
                    Message = "Invalid Transaction Type"
                }
            };

            var auditInfo = new AuditTransactionInfo
            {
                ServiceRequest = Json.Serialize(command.Request),
                MembershipNumber = command.Request.MemberNumber,
                TransactionSource = command.Request.TransactionChannel.IsNullOrEmpty() ? command.Request.TransactionSource : command.Request.TransactionChannel,
                CreatedBy = command.Request.CreatedBy,
                CreatedByEmailId = command.Request.CreatedBy,
                CreatedByUserName = command.Request.CreatedBy,
                TransactionID = command.Request.TransactionId,
                Status = Constants.Application.VRedActivityStatusProcessed,
                ActivityDate = command.Request.ActivityDate.Value.ToString(Constants.Application.VRedRequestDateFormat),
                TransactionProcessedDate = DateTime.UtcNow.ToString(Constants.Application.VRedRequestDateFormat)
            };

            if (command.Request.ActivityType.Is(Constants.Application.AccrualType))
            {
                auditInfo.TransactionType = Constants.Application.EarnActivityType;
                await _transactionDBRepository.AddTransactionAuditInfo(auditInfo.MembershipNumber, auditInfo.TransactionID, auditInfo.Status, auditInfo.TransactionType, auditInfo.ActivityDate, auditInfo.TransactionProcessedDate, auditInfo.TransactionSource, auditInfo.ServiceRequest,
                                                                       auditInfo.SiebelRequest, auditInfo.SiebelResponse, auditInfo.CreatedBy, auditInfo.CreatedByEmailId, auditInfo.CreatedByUserName);

                transactionResponse = await _mediator.Send(new AccrualVredTransactionRepoQuery()
                {
                    Request = command.Request,
                    RequestId = command.Id
                });
            }

            if (command.Request.ActivityType.Is(Constants.Application.RedemptionType))
            {
                auditInfo.TransactionType = Constants.Application.BurnActivityType;
                await _transactionDBRepository.AddTransactionAuditInfo(auditInfo.MembershipNumber, auditInfo.TransactionID, auditInfo.Status, auditInfo.TransactionType, auditInfo.ActivityDate, auditInfo.TransactionProcessedDate, auditInfo.TransactionSource, auditInfo.ServiceRequest,
                                                                     auditInfo.SiebelRequest, auditInfo.SiebelResponse, auditInfo.CreatedBy, auditInfo.CreatedByEmailId, auditInfo.CreatedByUserName);

                transactionResponse = await _mediator.Send(new RedemptionVredTransactionRepoQuery()
                {
                    Request = command.Request,
                    RequestId = command.Id
                });
            }

            if (transactionResponse.SuccessResponse != null)
            {
                var result = new Response<string>
                {
                    Status = Status.OK,
                    Value = Json.Serialize(transactionResponse.SuccessResponse)
                };

                await _mediator.Send(new ServiceResponseInsertCommand { Id = command.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.CreateTransactionEvent.ToString())) });

                var VRedResponseJson = Json.Serialize(transactionResponse.SuccessResponse);
                await _transactionDBRepository.UpdateVRedResponseAuditInfo(auditInfo.TransactionID, VRedResponseJson);
            }
            else
            {
                var result = new Response<string>
                {
                    Status = Status.BusinessException,
                    Value = Json.Serialize(transactionResponse.ErrorResponse)
                };

                await _mediator.Send(new ServiceResponseInsertCommand { Id = command.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.CreateTransactionEvent.ToString())) });

                var VRedResponseJson = Json.Serialize(transactionResponse.ErrorResponse);
                await _transactionDBRepository.UpdateVRedResponseAuditInfo(auditInfo.TransactionID, VRedResponseJson);
            }

            return transactionResponse;
        }
    }
}