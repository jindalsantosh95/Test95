using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionUpdate
{
    public class PromotionUpdateCommandHandler : IRequestHandler<PromotionUpdateCommand, Result<Response<string>>>
    {
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public PromotionUpdateCommandHandler(ICommonRefDBRepository commonRefDBRepository)
        {
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }
        public async Task<Result<Response<string>>> Handle(PromotionUpdateCommand request, CancellationToken cancellationToken)
        {
            string promotionUpdateResponse = await _commonRefDBRepository.UpdatePromotion(request.PromotionId, request.PromotionName, request.PartnerName, request.StartDate, request.EndDate, request.Active, request.CampaignLinked, request.InitiatedBy);
            return new Response<string>
            {
                Value = promotionUpdateResponse
            };
        }
    }
}
