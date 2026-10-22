using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionSearch
{
    public class GetPromotionCampaignCommandHandler : IRequestHandler<GetPromotionCampaignCommand, Result<Response<PromotionCampaignSearchResponse>>>
    {
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public GetPromotionCampaignCommandHandler(ICommonRefDBRepository commonRefDBRepository)
        {
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }
        public async Task<Result<Response<PromotionCampaignSearchResponse>>> Handle(GetPromotionCampaignCommand request, CancellationToken cancellationToken)
        {
            var promotionResponse = await _commonRefDBRepository.GetPromotionCampaignSearch(request.PromotionId);

            return promotionResponse;
        }
    }
}