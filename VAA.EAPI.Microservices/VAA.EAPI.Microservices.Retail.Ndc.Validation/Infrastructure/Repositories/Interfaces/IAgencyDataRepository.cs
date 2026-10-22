using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyValidate;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories.Interfaces
{
    public interface IAgencyDataRepository
    {
        Task<List<AgencyData>> GetAgencyDetails(AgencyValidateQuery requestData);
    }
}
