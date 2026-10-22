using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CampaignEnrolmentDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.CampaignEnrolmentDoc
{
    public class CampaignEnrolmentCreateCommand : CampaignEnrolmentCommandBase, IRequest<Result<Response<CampaignEnrolmentCommandResponse>>>
    {
        public CampaignEnrolmentCreateCommand() : base()
        {
            EventType = TransactionCommandType.CreateCampaignEnrolment;
        }
    }

    public class CampaignEnrolmentCreateCommandHandler : TransactionBaseCommandHandler<CampaignEnrolmentCommandResponse>, IRequestHandler<CampaignEnrolmentCreateCommand, Result<Response<CampaignEnrolmentCommandResponse>>>
    {
        public CampaignEnrolmentCreateCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<CampaignEnrolmentCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public Task<Result<Response<CampaignEnrolmentCommandResponse>>> Handle(CampaignEnrolmentCreateCommand request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }
    }


}
