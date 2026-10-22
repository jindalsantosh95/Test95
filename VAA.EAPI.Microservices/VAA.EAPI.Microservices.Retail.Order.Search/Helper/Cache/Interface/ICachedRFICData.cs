using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface
{
    public interface ICachedRFICData
    {
        Task<RFICTableEntity> GetCachedRFICData(string rowKey);
    }
}
