using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignRegister
{
    public class EmailEventRequestProcessCommand : CloudEvent, IRequest<Result<Response<string>>>
    {
    }
}
