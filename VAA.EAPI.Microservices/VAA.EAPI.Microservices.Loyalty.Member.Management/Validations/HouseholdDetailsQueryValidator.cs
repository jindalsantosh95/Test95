using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Validations
{
    public class HouseholdDetailsQueryValidator : AbstractValidator<HouseholdDetailsRetrievalQuery>
    {
        public HouseholdDetailsQueryValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}
