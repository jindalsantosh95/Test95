using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface
{
    public interface IFareFamily
    {
        Task<string> GetFareFamily(string rowKey);
        Task<System.Collections.Generic.IReadOnlyDictionary<string, string>> GetFareFamilies(
            System.Collections.Generic.IReadOnlyCollection<string> rowKeys, System.Threading.CancellationToken cancellationToken);
    }
}
