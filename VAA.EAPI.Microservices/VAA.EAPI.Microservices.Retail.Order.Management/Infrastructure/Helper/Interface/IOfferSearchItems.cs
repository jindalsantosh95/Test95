using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface
{
    public interface IOfferSearchItems
    {
        public Task<bool> GetOfferSearchItemsById(string offerRefId, string channelId);
    }
}
