using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.PartnerMembership
{
    public class PartnerMembershipCreateCommand : PartnerMembershipCommandBase, IRequest<Result<Response<PartnerMembershipCommandResponse>>>
    {
        public PartnerMembershipCreateCommand() : base()
        {
            EventType = TransactionCommandType.CreatePartnerMembership;
        }
    }
    public class PartnerMembershipCreateCommandHandler : TransactionBaseCommandHandler<PartnerMembershipCommandResponse>, IRequestHandler<PartnerMembershipCreateCommand, Result<Response<PartnerMembershipCommandResponse>>>
    {
        public PartnerMembershipCreateCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<PartnerMembershipCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public Task<Result<Response<PartnerMembershipCommandResponse>>> Handle(PartnerMembershipCreateCommand request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }
    }
}
