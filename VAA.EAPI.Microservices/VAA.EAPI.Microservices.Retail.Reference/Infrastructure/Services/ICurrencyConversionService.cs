using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Services
{
    public interface ICurrencyConversionService
    {
        Task<string> Handle(string request);
    }
}
