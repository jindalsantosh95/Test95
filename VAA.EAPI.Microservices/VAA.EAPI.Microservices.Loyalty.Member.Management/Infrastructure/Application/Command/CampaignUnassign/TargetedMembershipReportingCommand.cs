using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.CampaignUnassign;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.CampaignUnassign
{
    public class TargetedMembershipReportingCommand : TargetedMembershipData, IRequest<Result<Response<string>>>
    {

    }
}
