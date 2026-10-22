using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface ITokenB2CProvider<T>
    {
        Task<T> ValidateTokenAsync(string token);
    }
}
