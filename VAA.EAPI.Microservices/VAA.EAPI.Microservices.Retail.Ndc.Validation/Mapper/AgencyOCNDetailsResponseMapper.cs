using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.AgencyOCNDetails;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Mapper
{
    public static class AgencyOCNDetailsResponseMapper
    {
        public static IEnumerable<AgencyOCNDetails> ResponseMapper(IEnumerable<AgencyOCNData> agencyOCNData)
        {
            return agencyOCNData.Select(x =>
            {
                return new AgencyOCNDetails()
                {
                    EndpointDetails = new()
                    {
                        AuthType = x.AuthType,
                        ClientKey = x.ClientKey,
                        ClientSecret = x.ClientSecret,
                        Endpoint = x.EndPoint,
                        EndpointId = x.EndPointId,
                        SoapAction = x.SoapAction,
                        TokenUrl = x.TokenUrl
                    },
                    AgencyDetails = new()
                    {
                         RequestAgencyId = x.AgencyId,
                         RequestAgencyIdType = x.RequestAgencyIdType,
                         RequestOrgName = x.RequestOrgName
                    },
                    AggregatorDetails = new()
                    {
                        AgencyId = x.AgencyId,
                        AgencyIdType = x.AgencyIdType,
                        CompanyName = x.CompanyName,
                    },
                    Environment = x.Environment,
                    KeyType = x.KeyType
                };
            });
        }
    }
}
