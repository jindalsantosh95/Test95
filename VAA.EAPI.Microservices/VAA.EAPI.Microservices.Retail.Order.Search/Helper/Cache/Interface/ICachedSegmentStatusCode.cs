using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface
{
    public interface ICachedSegmentStatusCode
    {
        public Task<string> GetStatusCodeDescriptionAsync(string statusCode);

    }
}
