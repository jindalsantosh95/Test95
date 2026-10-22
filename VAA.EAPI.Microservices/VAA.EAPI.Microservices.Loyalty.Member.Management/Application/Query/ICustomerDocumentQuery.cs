using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public interface ICustomerDocumentQuery
    {
        Task<Result<Response<Customer>>> Handle(CustomerDocumentServiceQuery request, CancellationToken cancellationToken);
    }
}
