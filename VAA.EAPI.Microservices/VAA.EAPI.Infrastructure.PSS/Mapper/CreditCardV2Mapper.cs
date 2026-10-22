using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.CreditCardV2;
using VAA.EAPI.Infrastructure.PSS.Repository.Query;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class CreditCardV2Mapper
    {

        public static RetrieveEnhancedCreditCardAcceptanceRequestType MapRetrieveEnhancedCreditCardAcceptanceRequest(VerifyFraudCheckQuery verifyFraudCheckQuery)
        {
            var retrieveEnhancedCreditCardAcceptanceReq = new RetrieveEnhancedCreditCardAcceptanceRequestType
            {
                RequestInfo = new RequestInfoType()
                {
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    HostContextParams = new HostContextParamsType
                    {
                        PoolType = ConfigReader.Get(Constants.Environment.PoolType),
                        City = verifyFraudCheckQuery.Pos,
                    },
                    SessionId = verifyFraudCheckQuery.SessionId
                },
                Country = verifyFraudCheckQuery.Country,
                State = verifyFraudCheckQuery.State,
            };
            return retrieveEnhancedCreditCardAcceptanceReq;
        }

        public static string MapRetrieveEnhancedCreditCardAcceptanceResponse(RetrieveEnhancedCreditCardAcceptanceResponseType retrieveEnhancedCreditCardAcceptanceResponseType)
        {

            return (!retrieveEnhancedCreditCardAcceptanceResponseType.AdvisoryMessage.Equals(null) && retrieveEnhancedCreditCardAcceptanceResponseType.AdvisoryMessage.Length > 0) == true ? retrieveEnhancedCreditCardAcceptanceResponseType.AdvisoryMessage[0].FreeformText : string.Empty;

        }

    }

}