using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Helper.Cache.Interface
{
    public interface ICachedVSPartner
    {
        Task<string> GetCachedVSPartnerData(string partnerCode);
    }
}
