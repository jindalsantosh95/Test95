using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories
{
    public class CustomerDocumentRepoQuery : IRequest<Result<Response<Customers>>>
    {
        public string CustomerId { get; set; }

        public CustomerDocumentRepoQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}
