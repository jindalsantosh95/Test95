using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Infrastructure.Model.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.IFGAgentFopLookUp
{
    public static class IFGAgentFopLookUpRequestMapper
    {
        public static IFGAgentFopLookUpCommand MapIfgAgentLookUpRequest(Model.ValidationMS.ValidationRS validationMSRS)
        {
            var ifgAgentFopRequest = new IFGAgentFopLookUpCommand()
            {
                AgentCode = validationMSRS.ValidateResponse?.AgencyDetails.RequestAgencyId.Substring(0,(validationMSRS.ValidateResponse.AgencyDetails.RequestAgencyId.Length-1)),
                ClientID = Constants.Application.VSClientId,
                CountryCode = validationMSRS.ValidateResponse?.AgencyDetails.CountryCode

            };

            return ifgAgentFopRequest;
        
        }
    }
}
