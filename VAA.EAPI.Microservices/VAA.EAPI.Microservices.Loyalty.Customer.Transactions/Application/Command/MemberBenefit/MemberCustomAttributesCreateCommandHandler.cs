using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MemberBenefit
{
    public class MemberCustomAttributesCreateCommandHandler : TransactionBaseCommandHandler<MemberCustomAttributesCommandResponse>, IRequestHandler<MemberCustomAttributesCreateCommand, Result<Response<MemberCustomAttributesCommandResponse>>>, IMemberCustomAttributesCreateCommandHandler
    {
        public MemberCustomAttributesCreateCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<MemberCustomAttributesCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public Task<Result<Response<MemberCustomAttributesCommandResponse>>> Handle(MemberCustomAttributesCreateCommand request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }
    }
}
