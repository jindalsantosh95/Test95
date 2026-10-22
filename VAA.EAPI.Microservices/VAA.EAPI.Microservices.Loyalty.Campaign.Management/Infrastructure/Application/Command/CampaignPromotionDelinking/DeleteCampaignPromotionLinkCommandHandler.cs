using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignPromotionDelinking
{
    public class DeleteCampaignPromotionLinkCommandHandler : IRequestHandler<DeleteCampaignPromotionLinkCommand, Result<Response<string>>>
    {
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        public DeleteCampaignPromotionLinkCommandHandler(ICommonRefDBRepository commonRefDBRepository)
        {
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }
        public async Task<Result<Response<string>>> Handle(DeleteCampaignPromotionLinkCommand request, CancellationToken cancellationToken)
        {
            string campaignPromotionDelinkResponse = await _commonRefDBRepository.DeleteCampaignPromtionLink(request.CampaignId, request.PromotionId);
            return new Response<string>
            {
                Value = campaignPromotionDelinkResponse
            };
        }
    }
}
