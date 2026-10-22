using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.ContactHistoryDocument
{
    public class ContactHistoryRepoCreateCommand : IRequest<Result<Unit>>
    {
        public ContactHistory ContactHistoryEntities { get; set; }

        public ContactHistoryRepoCreateCommand(ContactHistory createDocument)
        {
            ContactHistoryEntities = createDocument;
        }
    }
}
