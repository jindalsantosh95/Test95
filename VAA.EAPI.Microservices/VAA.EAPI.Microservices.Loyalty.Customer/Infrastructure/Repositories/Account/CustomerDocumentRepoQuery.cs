using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Account
{
    public class CustomerDocumentRepoQuery : IRequest<Result<Response<Customers>>>
    {
        public CustomerDocumentRepoQuery(string cutomerId)
        {
            CustomerID = cutomerId;
        }
        public string CustomerID { get; set; }
    }
}
