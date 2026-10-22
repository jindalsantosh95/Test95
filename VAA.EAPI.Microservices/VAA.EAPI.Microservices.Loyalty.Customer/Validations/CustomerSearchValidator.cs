using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Validations
{
    public class CustomerSearchRetrieveValidator : AbstractValidator<CustomerSearchQuery>
    {
        public CustomerSearchRetrieveValidator()
        {
            RuleFor(x => x.MemberNumber).NotNull().NotEmpty();
        }
    }
}
