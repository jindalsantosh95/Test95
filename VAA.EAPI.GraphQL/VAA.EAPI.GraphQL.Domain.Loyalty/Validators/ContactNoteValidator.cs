using FluentValidation;
using System.Net.Http;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class ContactNoteValidator : AbstractValidator<ContactNote>
    {
        public ContactNoteValidator(HttpMethod verb)
        {
            if (verb != HttpMethod.Post)
            {
                RuleFor(x => x.Id).NotNull().NotEmpty();
            }
        }
    }
}
