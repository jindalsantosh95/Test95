using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignTypeRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignTypeRetrieval
{
    public class CampaignTypesRetrieveCommandHandler : IRequestHandler<CampaignTypesRetrieveCommand, Result<Response<CampaignTypeResponse>>>
    {
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public CampaignTypesRetrieveCommandHandler(ICommonRefDBRepository commonRefDBRepository)
        {
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }

        public async Task<Result<Response<CampaignTypeResponse>>> Handle(CampaignTypesRetrieveCommand request, CancellationToken cancellationToken)
        {
            var campaignTypes = await _commonRefDBRepository.GetCampaignTypes();

            return campaignTypes;
        }
    }
}
