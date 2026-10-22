using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.AddressHistory;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Validations
{
    public class AddressHistoryValidator : AbstractValidator<AddressHistoryQuery>
    {
        public AddressHistoryValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty();
        }
    }
}
