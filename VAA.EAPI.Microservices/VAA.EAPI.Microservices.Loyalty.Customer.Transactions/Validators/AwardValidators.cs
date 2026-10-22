using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class AwardCreateValidator : AbstractValidator<Award>
    {
        public AwardCreateValidator()
        {
            Include(new AwardProductIDValidator());
            Include(new AwardStartDateValidator());
        }
    }

    public class AwardDeleteValidator : AbstractValidator<Award>
    {
        public AwardDeleteValidator()
        {
            Include(new AwardIDValidator());
        }
    }
    public class AwardPatchValidator : AbstractValidator<Award>
    {
        public AwardPatchValidator()
        {
            Include(new AwardIDValidator());
        }
    }


    public class AwardProductIDValidator : AbstractValidator<Award>
    {
        public AwardProductIDValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty();
        }
    }

    public class AwardIDValidator : AbstractValidator<Award>
    {
        public AwardIDValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();

        }
    }

    public class AwardStartDateValidator : AbstractValidator<Award>
    {
        public AwardStartDateValidator()
        {
            RuleFor(x => x.StartDate).NotNull().NotEmpty();
        }
    }

    public class TopMemberAwardValidator : AbstractValidator<Award>
    {
        public TopMemberAwardValidator()
        {
            Include(new AwardIDValidator());
            Include(new AwardProductIDValidator());
        }
    }
}
