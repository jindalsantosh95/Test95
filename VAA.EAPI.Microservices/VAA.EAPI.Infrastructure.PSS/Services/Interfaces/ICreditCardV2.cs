using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Proxy.CreditCardV2;


namespace VAA.EAPI.Infrastructure.PSS.Services.Interfaces
{
    public interface ICreditCardV2
    {
        Task<RetrieveEnhancedCreditCardAcceptanceResponseType> RetrieveEnhancedCreditCardAcceptance(RetrieveEnhancedCreditCardAcceptanceRequestType retrieveEnhancedCreditCardAcceptanceRequestType);

    }
}
