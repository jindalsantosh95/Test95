using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers
{
    public class LTGAwardCloudEventHandler : IRequestHandler<LTGAwardCloudEvent, Result<Response<string>>>
    {
        private readonly IMediator _mediator;

        public LTGAwardCloudEventHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<string>>> Handle(LTGAwardCloudEvent eventRequest, CancellationToken cancellationToken)
        {
            return await _mediator.Send(Json.Deserialize<LTGEmailTriggerCommand>(Convert.ToString(eventRequest.Data)));
        }
    }
}
