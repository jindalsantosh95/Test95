using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.CreditCardV2;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Services
{
    public class CreditCardV2 : ICreditCardV2
    {
        readonly CreditCardClient _creditCardClient;
        public CreditCardV2(CreditCardClient creditCardClient)
        {
            _creditCardClient = creditCardClient ?? throw new ArgumentNullException(nameof(creditCardClient));
        }

        public async Task<RetrieveEnhancedCreditCardAcceptanceResponseType> RetrieveEnhancedCreditCardAcceptance(RetrieveEnhancedCreditCardAcceptanceRequestType retrieveEnhancedCreditCardAcceptanceRequestType)
        {
            try
            {
                if (_creditCardClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _creditCardClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }
                return (await _creditCardClient.RetrieveEnhancedCreditCardAcceptanceAsync(retrieveEnhancedCreditCardAcceptanceRequestType)).RetrieveEnhancedCreditCardAcceptanceResponse1;
            }
            catch (FaultException exception)
            {
                throw Air4FaultExceptionChecker.CreditCardV2Exception(exception);
            }
        }
    }
}
