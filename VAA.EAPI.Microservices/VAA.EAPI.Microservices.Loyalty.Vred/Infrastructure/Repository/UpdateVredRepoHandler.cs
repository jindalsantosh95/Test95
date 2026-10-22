using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository
{
    public class UpdateVredRepoHandler : IRequestHandler<UpdateVredRepoQuery, bool>
    {
        private readonly IMediator _mediator;

        public UpdateVredRepoHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        }

        public async Task<bool> Handle(UpdateVredRepoQuery req, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new UpdateVredServiceQuery(updateVredRequestMapper.ManageAccountRequestMapToVredRequest(req.UpdateVredMemberDetails), GetVredQuery(req.UpdateVredMemberDetails)));
        }

        private static string GetVredQuery(UpdateVredMemberDetailsRequest request)
        {
            return $"{Constants.Application.ParterUserId}{request.MemberNumber}{"&"}{Constants.Application.ParterUserId}{request.CustomerId}";
        }
    }
}
