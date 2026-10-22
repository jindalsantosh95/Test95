using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.AgencyValidate;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyValidate
{
    public class AgencyValidateQuery : AgencyValidateRequest, IRequest<Result<Response<AgencyValidateResponse>>>
    {
        public string AgentSubscriptionKey { get; set; }
        public string Environment { get; set; }
        public string RequestAgencyType { get; set; }

        public AgencyValidateQuery(string agentSubscriptionKey, string environment, string requestAgencyType)
        {
            AgentSubscriptionKey = agentSubscriptionKey;
            Environment = environment;
            RequestAgencyType = requestAgencyType;
        }
    }
}
