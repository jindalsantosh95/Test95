using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer
{
    public interface IGetCustTransDocumentService<T>
    {
        Task<T> GetCosmosDoc(string docType, string customerId);
    }
}
