using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface
{
    public interface ICachedFlightProduct
    {
        Task<Product> GetFlightProductAsync();
    }
}
