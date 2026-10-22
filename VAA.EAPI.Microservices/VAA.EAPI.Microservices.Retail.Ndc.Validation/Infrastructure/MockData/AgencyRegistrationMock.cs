using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.AgencyValidate;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.MockData
{
    public class AgencyRegistrationMock
    {
        public static List<AgencyValidateResponse> GetAgencyMockData()
        {
            return new List<AgencyValidateResponse>
            {

                new() //Agency Coming Directly
                {
                    AgencyDetails = new AgencyDetails
                    {
                        RequestAgencyId = "1199",
                        RequestAgencyIdType = "IATA",
                        RequestOrgName = "Skyscanner"
                    },
                    AggregatorDetails = new AggregatorDetails
                    {
                        AgencyId = "1199",
                        AgencyIdType = "IATA",
                        CompanyName = "Skyscanner"
                    },
                    KeyStatus = "ACTIVE",
                    Environment = "PREPROD"
                },

                new() //corporate coming directly
                {
                    AgencyDetails = new AgencyDetails
                    {
                        RequestAgencyId = "3406",
                        RequestAgencyIdType = "IATA",
                        RequestOrgName = "Skyscanner"
                    },
                    AggregatorDetails = new AggregatorDetails
                    {
                        AgencyId = "3406",
                        AgencyIdType = "IATA",
                        CompanyName = "Skyscanner"
                    },
                    CorporateDetails = new CorporateDetails
                    {
                        CorporateId = "VS9000000000347"
                    },
                    KeyStatus = "ACTIVE",
                    Environment = "PREPROD"
                },

                new() //agent coming via aggregator
                {
                    AgencyDetails = new AgencyDetails
                    {
                        RequestAgencyId = "3547",
                        RequestAgencyIdType = "IATA",
                        RequestOrgName = "Skyscanner"
                    },
                    AggregatorDetails = new AggregatorDetails
                    {
                        AgencyId = "4857",
                        AgencyIdType = "IATA",
                        CompanyName = "Travelfused"
                    },
                    KeyStatus = "ACTIVE",
                    Environment = "PREPROD"
                },

                new() //corporate coming via aggregator
                {
                    AgencyDetails = new AgencyDetails
                    {
                        RequestAgencyId = "123456",
                        RequestAgencyIdType = "IATA",
                        RequestOrgName = "Skyscanner"
                    },
                    AggregatorDetails = new AggregatorDetails
                    {
                        AgencyId = "6789",
                        AgencyIdType = "IATA",
                        CompanyName = "Travelfused"
                    },
                    CorporateDetails = new CorporateDetails
                    {
                        CorporateId = "VS9000000000348"
                    },
                    KeyStatus = "ACTIVE",
                    Environment = "PREPROD"
                }
            };
        }
    }
}
