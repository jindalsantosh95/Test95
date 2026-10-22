using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces
{
    public interface IAgentTktAuthSvc
    {
        Task<AgentTktAuthResponse> GetAgentTktAuthResponse(string agencyID, string requestAgencyType, string environment);
        Task<AgentTktAuthResponse> UpdateAgentTktAuthResponse(string agencyID, string requestAgencyType, string environment);
    }
}
