using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.SFPD;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class LoyaltySyncSFPDValidator : AbstractValidator<SFPDSyncMutationRequest>
    {
        public LoyaltySyncSFPDValidator()
        {
            RuleFor(x => x.FcNumber).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.CustomerId));
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.FcNumber));
            RuleFor(x => x.FirstName).NotNull().NotEmpty();
            RuleFor(x => x.LastName).NotNull().NotEmpty();
            RuleFor(x => x.DateOfBirth).NotNull().NotEmpty();
            RuleFor(x => x.Gender).NotNull().NotEmpty();
            RuleFor(x => x.EnrollmentChannel).NotNull().NotEmpty();
        }
    }
}
