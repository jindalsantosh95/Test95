using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyOCNDetails;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories
{
    public class AgencyOCNDetailsRepository(IAgencyOCNDetailsService agencyOCNDetails) : IAgencyOCNDetailsRepository
    {
        private readonly IAgencyOCNDetailsService _agencyOCNDetailsService = agencyOCNDetails;

        public async Task<IEnumerable<AgencyOCNData>> GetAgencyOCNDetails(AgencyOCNDetailsQuery requestData)
        {
            return await _agencyOCNDetailsService.GetAgencyOCNDetails(requestData);
        }
    }
}
