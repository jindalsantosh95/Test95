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
    public class PartnerMembershipPatchCommand : PartnerMembershipCommandBase, IRequest<Result<Response<PartnerMembershipCommandResponse>>>
    {
        public PartnerMembershipPatchCommand()
        {
            EventType = TransactionCommandType.UpdatePartnerMembership;

        }
    }
    public class PartnerMembershipPatchCommandHandler : TransactionBaseCommandHandler<PartnerMembershipCommandResponse>, IRequestHandler<PartnerMembershipPatchCommand, Result<Response<PartnerMembershipCommandResponse>>>
    {
        public PartnerMembershipPatchCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<PartnerMembershipCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public Task<Result<Response<PartnerMembershipCommandResponse>>> Handle(PartnerMembershipPatchCommand request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }
    }

}
