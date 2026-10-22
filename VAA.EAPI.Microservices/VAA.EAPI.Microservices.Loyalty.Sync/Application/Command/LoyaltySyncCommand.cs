using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Application.Command
{
    public class LoyaltySyncEventProcessCommand : CloudEvent, IRequest<Result<Unit>>
    {
        public new LoyaltySyncRequestData Data { get; set; }
    }
}
