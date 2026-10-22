using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Interface
{
    public interface ICurrencyConversionCommand
    {
        Task Handle();
    }
}
