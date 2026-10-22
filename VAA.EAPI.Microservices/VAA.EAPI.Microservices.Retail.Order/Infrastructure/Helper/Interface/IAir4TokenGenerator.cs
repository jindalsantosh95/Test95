using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Helper.Interface
{
    public interface IAir4TokenGenerator
    {
        Task<Result<Response<string>>> GetToken(string clientId, string clientSecret);
    }
}
