using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.ContactNotes
{
    public class ContactNotesQuery : TransactionBase, IRequest<Result<Response<ContactNotesResponse>>>
    { }
}
