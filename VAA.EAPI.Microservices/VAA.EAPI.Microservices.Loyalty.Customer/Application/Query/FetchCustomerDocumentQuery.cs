using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query
{
    public class FetchCustomerDocumentQuery : IRequest<Result<Response<Customers>>>
    {
        public string CustomerId { get; set; }

        public FetchCustomerDocumentQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}
