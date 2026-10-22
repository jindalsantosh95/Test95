using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.AgencyValidate;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Mapper
{
    public static class AgencyValidateResponseMapper
    {
        public static AgencyValidateResponse MapAgencyDetails(Application.Query.AgencyValidate.AgencyValidateQuery requestData, List<AgencyData> agencyList)
        {
            if (requestData?.Parties?.Participants?.Count() > 1)
            {
                var distributorAgencyId = requestData.Parties.Participants.Where(x => x.PartyType == EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Distributor).FirstOrDefault()?.AgencyID;
                var sellerAgencyId = requestData.Parties.Participants.Where(x => x.PartyType == EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller).FirstOrDefault()?.AgencyID;
                var aggregatorAgentDetails = agencyList?.Where(x => x.AgencyId == distributorAgencyId).FirstOrDefault();
                var sellerAgentDetails = agencyList?.Where(x => x.AgencyId == sellerAgencyId).FirstOrDefault();

                sellerAgentDetails ??= agencyList.FirstOrDefault();

                return MapResponse(sellerAgentDetails, aggregatorAgentDetails);
            }
            else
            {
                return MapResponse(agencyList.First(x=>x.KeyType == requestData.KeyType), null);
            }
        }

        private static AgencyValidateResponse MapResponse(AgencyData sellerAgentDetails, AgencyData aggregatorAgentDetails)
        {
            var agentResponse = new List<AgencyValidateResponse>
            {
                new AgencyValidateResponse()
                {
                    CorporateDetails = new CorporateDetails()
                        {
                            CorporateAccountCode = sellerAgentDetails.CorporateAccountCode,
                            CorporateId = sellerAgentDetails.CorporateId,
                        },

                    AgencyPermissions = new AgencyPermissions()

                        {
                            AirlineTicketingAuthorityInd = sellerAgentDetails.AirlineTicketingAuthorityInd ?? false,
                            BspActiveStatusInd = sellerAgentDetails.BspActiveStatusInd ?? false,
                            CardFopAllowedInd = sellerAgentDetails.CardFopAllowedInd ?? false,
                            CashFopAllowedInd = sellerAgentDetails.CashFopAllowedInd ?? false,
                            EasyPayFopAllowedInd = sellerAgentDetails.EasyPayFopAllowedInd ?? false,
                            PciDssComplianceInd = sellerAgentDetails.PciDssComplianceInd ?? false
                        },
                            Environment = sellerAgentDetails.Environment,
                            KeyStatus = sellerAgentDetails.KeyStatus,
                            KeyType = aggregatorAgentDetails?.KeyType ?? sellerAgentDetails.KeyType,
                            OCNFlag = sellerAgentDetails.OCNFlag == 1
                }
            };

            if (sellerAgentDetails.RequestAgencyIdType == "IATA" || sellerAgentDetails.RequestAgencyIdType == "TIDS")
            {
                var agencyDetails = new AgencyDetails()
                {
                    AccountCode = sellerAgentDetails.AccountCode,
                    CountryCode = sellerAgentDetails.CountryCode,
                    RequestAgencyId = sellerAgentDetails.RequestAgencyId,
                    RequestAgencyIdType = sellerAgentDetails.RequestAgencyIdType,
                    RequestOrgName = sellerAgentDetails.RequestOrgName,
                    PocCode = sellerAgentDetails.PocCode
                };

                agentResponse.First().AgencyDetails = agencyDetails;
            }

            if (aggregatorAgentDetails is not null && aggregatorAgentDetails.BusinessType == "AGGREGATOR")
            {
                var agencyDetails = new AgencyDetails()
                {
                    AccountCode = aggregatorAgentDetails.AccountCode,
                    CountryCode = aggregatorAgentDetails.CountryCode,
                    RequestAgencyId = aggregatorAgentDetails.RequestAgencyId,
                    RequestAgencyIdType = aggregatorAgentDetails.RequestAgencyIdType,
                    RequestOrgName = aggregatorAgentDetails.RequestOrgName,
                    PocCode = aggregatorAgentDetails.PocCode,
                };

                var aggregatorDetails = new AggregatorDetails()

                {
                    AgencyId = aggregatorAgentDetails.AgencyId,
                    AgencyIdType = aggregatorAgentDetails.AgencyIdType,
                    CompanyName = aggregatorAgentDetails.CompanyName,
                    PocCode = aggregatorAgentDetails.PocCode,
                };

                agentResponse.First().AgencyDetails = agencyDetails;
                agentResponse.First().AggregatorDetails = aggregatorDetails;
            }

            return agentResponse.FirstOrDefault();
        }

    }

}
