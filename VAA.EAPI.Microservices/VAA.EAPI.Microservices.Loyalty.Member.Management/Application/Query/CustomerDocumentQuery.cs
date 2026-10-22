using CSharpFunctionalExtensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class CustomerDocumentQuery : ICustomerDocumentQuery
    {
        private readonly CustomerDocumentServiceQueryHandler _customerDocumentServiceQueryHandler;

        public CustomerDocumentQuery(CustomerDocumentServiceQueryHandler customerDocumentServiceQueryHandler)
        {
            _customerDocumentServiceQueryHandler = customerDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(customerDocumentServiceQueryHandler));
        }

        public Task<Result<Response<Customer>>> Handle(CustomerDocumentServiceQuery request, CancellationToken cancellationToken) => _customerDocumentServiceQueryHandler.Handle(request, cancellationToken);
    }
}
