using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Passenger.Application.Commands
{
    public class ManageMultipleInfantCommand : ManageMultipleInfantV2RequestType, IRequest<Result<Response<ManageMultipleInfantV2Response>>>
    {
        public string ChannelId { get; set; }

    }
}
