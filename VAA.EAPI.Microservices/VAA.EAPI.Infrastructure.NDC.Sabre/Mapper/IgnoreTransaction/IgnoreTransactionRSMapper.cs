using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.IgnoreTransaction;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.IgnoreTransaction
{
    public class IgnoreTransactionRSMapper
    {
        public static bool IgnoreTransactionResponseMapper(IgnoreTransactionRQResponse response)
        {
            return response?.IgnoreTransactionRS?.ApplicationResults.status == CompletionCodes.Complete ? true : false;
        }
    }
}
