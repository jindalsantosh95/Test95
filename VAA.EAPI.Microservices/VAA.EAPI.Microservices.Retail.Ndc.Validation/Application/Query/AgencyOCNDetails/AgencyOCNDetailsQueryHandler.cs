using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyOCNDetails;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.OCNEndpoints
{
    public class AgencyOCNDetailsQueryHandler(IAgencyOCNDetailsRepository agencyOCNDetailsRepository) : IRequestHandler<AgencyOCNDetailsQuery, Response<IEnumerable<Domain.AgencyOCNDetails.AgencyOCNDetails>>>
    {
        private readonly IAgencyOCNDetailsRepository _agencyOCNDetailsRepository = agencyOCNDetailsRepository;

        public async Task<Response<IEnumerable<Domain.AgencyOCNDetails.AgencyOCNDetails>>> Handle(AgencyOCNDetailsQuery request, CancellationToken cancellationToken)
        {
            var agencyOCNData = await _agencyOCNDetailsRepository.GetAgencyOCNDetails(request);

            return new Response<IEnumerable<Domain.AgencyOCNDetails.AgencyOCNDetails>>
            {
                Status = Status.OK,
                Value = AgencyOCNDetailsResponseMapper.ResponseMapper(agencyOCNData)
            };
        }
    }
}
