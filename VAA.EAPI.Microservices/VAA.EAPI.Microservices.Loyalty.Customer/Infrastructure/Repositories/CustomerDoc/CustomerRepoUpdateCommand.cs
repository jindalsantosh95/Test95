using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerDoc
{
    public class CustomerRepoUpdateCommand : IRequest<Result<Unit>>
    {
        public Customers CustomerDocument { get; set; }

        public CustomerRepoUpdateCommand(Customers updatedDocument)
        {
            CustomerDocument = updatedDocument;
        }
    }
}