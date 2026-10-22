using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyOCNDetails;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories.Interfaces
{
    public interface IAgencyOCNDetailsRepository
    {
        Task<IEnumerable<AgencyOCNData>> GetAgencyOCNDetails(AgencyOCNDetailsQuery requestData);
    }
}
