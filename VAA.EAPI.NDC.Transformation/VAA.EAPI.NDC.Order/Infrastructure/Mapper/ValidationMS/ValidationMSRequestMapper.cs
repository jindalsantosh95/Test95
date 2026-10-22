using System.Linq;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS
{
    public class ValidationMSRequestMapper
    {
        public static AgencyValidateRequest MapValidationRequest(Parties partiesRQ, CorporateAccount corporateAccount, bool checkTktAuthority = false, bool ocnValidation = false)
        {
            AgencyValidateRequest validationRQ = new()
            {
                Parties = partiesRQ,
                Keytype = partiesRQ.Participants.Count() > 1 ? Constants.Application.KeyTypeIndirect : Constants.Application.KeyTypeDirect,
                CheckAgentTktAuthority = checkTktAuthority,
                OCNValidation = ocnValidation
            };

            if (corporateAccount != null)
            {
                validationRQ.Parties.CorporateAccount = new CorporateAccount()
                {
                    CorporateCode = corporateAccount.CorporateCode,
                    AccountName = corporateAccount.AccountName
                };
            }

            return validationRQ;
        }
    }
}
