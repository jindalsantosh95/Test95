using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionCampaignSearch
{
    public class GetPromotionCommandHandler : IRequestHandler<GetPromotionCommand, Result<Response<PromotionSearchResponse>>>
    {
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public GetPromotionCommandHandler(ICommonRefDBRepository commonRefDBRepository)
        {
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }
        public async Task<Result<Response<PromotionSearchResponse>>> Handle(GetPromotionCommand request, CancellationToken cancellationToken)
        {
            var promotionResponse = await _commonRefDBRepository.GetPromotionSearch(request.PromotionId, request.PromotionName, request.StartDateF, request.EndDateF, request.StartDateT, request.EndDateT, request.PartnerName, request.Active);

            return promotionResponse;
        }
    }
}