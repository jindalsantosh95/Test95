using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ContactHistoryCommand
{
    public class ContactHistoryCreateCommand : IRequest<Result<Unit>>
    {
        public ContactHistory ContactHistoryEntities { get; set; }

        public ContactHistoryCreateCommand(ContactHistory createDocument)
        {
            ContactHistoryEntities = createDocument;
        }
    }
}
