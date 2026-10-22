using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignRegister
{
    public class CampaignRegisterRequestCommand : CamapignRegisterRequestData, IRequest<Result<Response<string>>>
    {
    }
}
