using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Application.Command
{
    public class SFPDSyncCommand : IRequest<Result<Response<SFPDSyncCommandResponse>>>
    {
        public LoyaltySyncRequestData SfpdData { get; set; }
    }
}

