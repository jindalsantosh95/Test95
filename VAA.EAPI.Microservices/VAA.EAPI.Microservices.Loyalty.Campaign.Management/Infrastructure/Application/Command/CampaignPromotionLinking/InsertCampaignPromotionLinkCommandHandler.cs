using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignPromotionLinking
{
    public class InsertCampaignPromotionLinkCommandHandler : IRequestHandler<InsertCampaignPromotionLinkCommand, Result<Response<string>>>
    {
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        public InsertCampaignPromotionLinkCommandHandler(ICommonRefDBRepository commonRefDBRepository)
        {
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }
        public async Task<Result<Response<string>>> Handle(InsertCampaignPromotionLinkCommand request, CancellationToken cancellationToken)
        {
            string campaignPromotionLinkResponse = await _commonRefDBRepository.InsertCampaignPromtionLink(request.CampaignId, request.PromotionId, request.CreatedBy);
            return new Response<string>
            {
                Value = campaignPromotionLinkResponse
            };
        }
    }
}
