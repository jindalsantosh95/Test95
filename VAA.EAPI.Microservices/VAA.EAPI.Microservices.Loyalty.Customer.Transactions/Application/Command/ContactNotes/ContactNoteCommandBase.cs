using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ContactNotes
{
    public class ContactNoteCommandBase : TransactionBaseCommand
    {
        public ContactNote ContactNote { get; set; }

        public ContactNoteCommandBase() : base()
        {
            Subject = ConfigReader.Get(Constants.EnvironmentSettings.ContactNoteEventSubject);
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.ContactNoteDocType);
        }

        public static async Task<IEnumerable<ContactNote>> GetListOfContactNotes(IMediator mediator, string customerId, CancellationToken cancellationToken)
        {
            return (await mediator.Send(new ContactNotesQuery() { CustomerId = customerId, DocType = ConfigReader.Get(Constants.EnvironmentSettings.ContactNoteDocType) }, cancellationToken))
                                 .Value.Value.ContactNotes;
        }
    }
}
