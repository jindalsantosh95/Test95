using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionCreate
{
    public class PromotionCreateCommandHandler : IRequestHandler<PromotionCreateCommand, Result<Response<string>>>
    {
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public PromotionCreateCommandHandler(ICommonRefDBRepository commonRefDBRepository)
        {
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }
        public async Task<Result<Response<string>>> Handle(PromotionCreateCommand request, CancellationToken cancellationToken)
        {
            string promotionCreateResponse = await _commonRefDBRepository.CreatePromotion(request.PromotionName, request.PartnerName, request.StartDate, request.EndDate, request.Active, request.InitiatedBy);
            return new Response<string>
            {
                Value = promotionCreateResponse
            };
        }
    }
}
