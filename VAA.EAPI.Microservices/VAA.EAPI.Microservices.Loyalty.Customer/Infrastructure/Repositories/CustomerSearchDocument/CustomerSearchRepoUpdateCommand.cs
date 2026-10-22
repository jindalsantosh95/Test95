using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument
{
    public class CustomerSearchRepoUpdateCommand : IRequest<Result<Unit>>
    {
        public CustomerSearch CustomerSearchDocument { get; set; }

        public CustomerSearchRepoUpdateCommand(CustomerSearch updatedDocument)
        {
            CustomerSearchDocument = updatedDocument;
        }
    }
}