using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyValidate;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories
{
    public class AgentTktAuthRetrieveQuery : IRequest<AgentTktAuthResponse>
    {
        public AgencyValidateQuery RequestData { get; set; }
    }
}
