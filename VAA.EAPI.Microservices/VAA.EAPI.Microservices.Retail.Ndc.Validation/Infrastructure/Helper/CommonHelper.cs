using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyValidate;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.AgencyValidate;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Helper
{
    public class CommonHelper
    {
        public static string GetAgencyID(AgencyValidateRequest agencyValidateRequest)
        {
            return agencyValidateRequest?.Parties?.Participants?.Count() == 1
                      ? agencyValidateRequest?.Parties?.Participants.FirstOrDefault(participant => participant.PartyType == Participant.PartyTypeEnum.Seller)?.AgencyID
                      : $"{agencyValidateRequest?.Parties?.Participants.FirstOrDefault(participant => participant.PartyType == Participant.PartyTypeEnum.Seller)?.AgencyID}-" +
                      $"{agencyValidateRequest?.Parties?.Participants.FirstOrDefault(participant => participant.PartyType == Participant.PartyTypeEnum.Distributor)?.AgencyID}";
        }

        public static string GenerateCacheKey(AgencyValidateQuery requestData)
        {
            var agencyID = requestData.Parties.Participants.FirstOrDefault(p => p.PartyType == Participant.PartyTypeEnum.Seller).AgencyID;
            var envirnoment = requestData.Environment;
            var keyType = requestData.KeyType;
            var corporateCode = requestData.Parties.CorporateAccount?.CorporateCode;

            return $"{agencyID}-{envirnoment}-{keyType}{(string.IsNullOrEmpty(corporateCode) ? string.Empty : $"-{corporateCode}")}";
        }
    }
}
