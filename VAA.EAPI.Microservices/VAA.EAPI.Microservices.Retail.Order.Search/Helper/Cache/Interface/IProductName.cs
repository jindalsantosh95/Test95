using System.Threading.Tasks;
namespace VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface
{
    public interface IProductName
    {
        public Task<string> GetProductName(string rowKey);
    }
}
