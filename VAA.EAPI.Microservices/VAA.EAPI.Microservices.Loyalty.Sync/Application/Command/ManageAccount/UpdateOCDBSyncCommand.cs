using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Application.Command.ManageAccount
{
    public class UpdateOCDBSyncCommand : CloudEvent, IRequest<Result<bool>>
    {
        public new LoyaltySyncRequestData Data { get; set; }
    }
}