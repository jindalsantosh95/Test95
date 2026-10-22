using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface
{
    public interface ICachedPriceClass
    {
        Task<PriceClassTableEntity> GetCachedPriceClassData(string rowKey);
    }
}
