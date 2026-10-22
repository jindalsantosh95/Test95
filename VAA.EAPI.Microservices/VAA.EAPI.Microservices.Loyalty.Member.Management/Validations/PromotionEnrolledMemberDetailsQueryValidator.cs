using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Validations
{
    public class PromotionEnrolledMemberDetailsQueryValidator : AbstractValidator<PromotionEnrolledMemberDetailsQuery>
    {
        public PromotionEnrolledMemberDetailsQueryValidator()
        {
            RuleFor(x => x.PromotionId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}
