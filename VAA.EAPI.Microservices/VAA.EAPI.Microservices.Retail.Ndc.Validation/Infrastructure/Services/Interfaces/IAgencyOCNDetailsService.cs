using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyOCNDetails;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces
{
    public interface IAgencyOCNDetailsService
    {
        Task<IEnumerable<AgencyOCNData>> GetAgencyOCNDetails(AgencyOCNDetailsQuery requestData);
    }
}
