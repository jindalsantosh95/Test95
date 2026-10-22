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
    public class MemberCustomAttributesPatchCommandHandler : TransactionBaseCommandHandler<MemberCustomAttributesCommandResponse>, IRequestHandler<MemberCustomAttributesPatchCommand, Result<Response<MemberCustomAttributesCommandResponse>>>, IMemberCustomAttributesPatchCommandHandler
    {
        public MemberCustomAttributesPatchCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<MemberCustomAttributesCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public async Task<Result<Response<MemberCustomAttributesCommandResponse>>> Handle(MemberCustomAttributesPatchCommand request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
